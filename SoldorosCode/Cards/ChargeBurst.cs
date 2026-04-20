using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 차지 버스트 — 고급 공격. 피해 5만큼 2번. 적 취약 2배. 소멸.
// 강화: 피해 5 → 7.
public sealed class ChargeBurst : SoldorosCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust,
    };

    protected override bool ShouldGlowGoldInternal =>
        base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<VulnerablePower>()) ?? false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<VulnerablePower>(),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
    };

    public ChargeBurst() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(2)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 취약 2배: 현재 스택만큼 추가 적용 (현재 + 현재 = 2×)
        int current = cardPlay.Target.IsAlive ? cardPlay.Target.GetPowerAmount<VulnerablePower>() : 0;
        if (current > 0)
        {
            await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, current, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);   // 5 → 7
    }
}
