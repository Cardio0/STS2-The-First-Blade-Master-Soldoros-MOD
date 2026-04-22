using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Powers;

// 류심 강 파워 — 무색 공격 카드 피해량 +Amount.
public sealed class FlowingStanceForcePower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? card)
    {
        if (base.Owner != dealer) return 0m;
        if (card == null) return 0m;
        if (card.Type != CardType.Attack) return 0m;
        if (!card.VisualCardPool.IsColorless) return 0m;
        return base.Amount;
    }
}
