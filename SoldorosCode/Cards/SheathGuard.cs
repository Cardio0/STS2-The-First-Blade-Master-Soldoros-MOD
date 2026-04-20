using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

public sealed class SheathGuard : SoldorosCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(12m, ValueProp.Move),
    };

    public SheathGuard() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);

        CardModel? selected = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Owner,
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
            filter: null,
            source: this)).FirstOrDefault();

        // 선택한 카드를 뽑을 카드 더미에 추가한 뒤 뽑을 카드 더미만 셔플.
        // CardPileCmd.Shuffle 은 버린 카드 더미까지 섞으므로 사용하지 않음.
        if (selected != null)
        {
            await CardPileCmd.Add(selected, PileType.Draw);
            PileType.Draw.GetPile(base.Owner).RandomizeOrderInternal(
                base.Owner, base.Owner.RunState.Rng.Shuffle, base.CombatState!);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Block"].UpgradeValueBy(3m);   // 12 → 15
    }
}
