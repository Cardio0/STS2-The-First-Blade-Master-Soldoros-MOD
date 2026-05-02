using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Cards;

// 무기의 달인 — 고급 스킬. 멀티 전용. 1(→0) 코스트.
// 다른 플레이어 카드 2장 뽑기, 에너지 1 획득. (자신 제외)
// 자신 손패 카드 1장을 뽑을 카드 더미 맨 위에 놓음. 소멸.
public sealed class GrandmasterAtArms : SoldorosCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get { yield return base.EnergyHoverTip; }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1),
    };

    public GrandmasterAtArms() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState is null) return;

        List<Creature> others = base.CombatState.GetTeammatesOf(base.Owner.Creature)
            .Where(c => c != null && c.IsAlive && c.IsPlayer && c != base.Owner.Creature)
            .ToList();

        foreach (Creature other in others)
            await CardPileCmd.Draw(choiceContext, 2, other.Player!);

        foreach (Creature other in others)
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, other.Player!);

        CardModel? selected = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Owner,
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
            filter: null,
            source: this)).FirstOrDefault();

        if (selected != null)
            await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);   // 1 → 0
    }
}
