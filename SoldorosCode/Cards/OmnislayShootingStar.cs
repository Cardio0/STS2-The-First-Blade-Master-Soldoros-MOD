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

// 극 귀검술:유성락 — 공격. 꿰뚫기. 피해 10(→12). 막히지 않은 피해량만큼 방어도 획득. 소멸(강화 시 소멸 제거).
public sealed class OmnislayShootingStar : SoldorosCard
{
    // 취약 상태인 적이 있으면 금색 테두리 강조
    protected override bool ShouldGlowGoldInternal =>
        base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<VulnerablePower>()) ?? false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust,
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        new HoverTip(
            new LocString("card_keywords", "SOLDOROS-PIERCE.title"),
            new LocString("card_keywords", "SOLDOROS-PIERCE.description")
        ),
        HoverTipFactory.Static(StaticHoverTip.Block),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PierceDamageVar(10m),
    };

    public OmnislayShootingStar() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 꿰뚫기: 취약 스택 소비 → 피해 배율 적용
        int stacks = cardPlay.Target.GetPower<VulnerablePower>()?.Amount ?? 0;
        if (stacks > 0)
            await PowerCmd.Remove<VulnerablePower>(cardPlay.Target);

        int str = base.Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0;
        int vigor = base.Owner.Creature.GetPower<VigorPower>()?.Amount ?? 0;
        decimal finalDamage = base.DynamicVars.Damage.BaseValue * (1 + stacks) + (decimal)(str + vigor) * stacks;

        var cmd = await DamageCmd.Attack(finalDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        int unblocked = cmd.Results.Sum(r => r.UnblockedDamage);
        if (unblocked > 0)
            await CreatureCmd.GainBlock(base.Owner.Creature, (decimal)unblocked, ValueProp.Unpowered, cardPlay);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);   // 10 → 12
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
