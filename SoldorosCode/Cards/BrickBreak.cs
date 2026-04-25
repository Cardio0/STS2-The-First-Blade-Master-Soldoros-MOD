using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Cards;

// 깨뜨리기 — 고급 스킬. 0 코스트.
// 대상 적의 모든 인공물과 방어도 제거. 약화 2(→3) 부여. 소멸.
public sealed class BrickBreak : SoldorosCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<ArtifactPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Weak", 2m),
    };

    public BrickBreak() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 방어도 전부 제거
        await CreatureCmd.LoseBlock(cardPlay.Target, cardPlay.Target.Block);

        // 인공물 전부 제거
        if (cardPlay.Target.HasPower<ArtifactPower>())
            await PowerCmd.Remove<ArtifactPower>(cardPlay.Target);

        // 약화 부여
        await PowerCmd.Apply<WeakPower>(cardPlay.Target, base.DynamicVars["Weak"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Weak"].UpgradeValueBy(1m);   // 2 → 3
    }
}
