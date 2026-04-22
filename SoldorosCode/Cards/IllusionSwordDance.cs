using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 환영검무 — 고급 공격. X 코스트. 꿰뚫기. 피해 5(→6)만큼 X번.
public sealed class IllusionSwordDance : SoldorosCard
{
    // 취약 상태인 적이 있으면 금색 테두리 강조
    protected override bool ShouldGlowGoldInternal =>
        base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<VulnerablePower>()) ?? false;

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        new HoverTip(
            new LocString("card_keywords", "SOLDOROS-PIERCE.title"),
            new LocString("card_keywords", "SOLDOROS-PIERCE.description")),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PierceDamageVar(5m),
    };

    public IllusionSwordDance() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int xValue = ResolveEnergyXValue();
        if (xValue <= 0) return;

        // 꿰뚫기: 취약 스택 전부 소비 → 소비한 스택 수만큼 피해 100%씩 증폭
        int stacks = cardPlay.Target.GetPower<VulnerablePower>()?.Amount ?? 0;
        if (stacks > 0)
            await PowerCmd.Remove<VulnerablePower>(cardPlay.Target);

        int str = base.Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0;
        int vigor = base.Owner.Creature.GetPower<VigorPower>()?.Amount ?? 0;
        decimal finalDamage = base.DynamicVars.Damage.BaseValue * (1 + stacks) + (decimal)(str + vigor) * stacks;

        await DamageCmd.Attack(finalDamage)
            .WithHitCount(xValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(1m);   // 5 → 6
    }
}
