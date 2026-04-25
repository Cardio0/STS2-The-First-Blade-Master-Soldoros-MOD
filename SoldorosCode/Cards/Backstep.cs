using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Soldoros.SoldorosCode.Powers;

namespace Soldoros.SoldorosCode.Cards;

// 백스텝 — 일반 스킬. 0 코스트. 소멸.
// 이번 턴 동안 민첩 3 획득. 손에서 카드 1장을 뽑을 카드 더미 맨 위에 놓음.
// 업그레이드: 민첩 3 → 4.
public sealed class Backstep : SoldorosCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DexterityPower>(),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DexterityPower>(3m),
    };

    public Backstep() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<BackstepPower>(base.Owner.Creature, base.DynamicVars.Dexterity.BaseValue, base.Owner.Creature, this);

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
        base.DynamicVars.Dexterity.UpgradeValueBy(1m);   // 3 → 4
    }
}
