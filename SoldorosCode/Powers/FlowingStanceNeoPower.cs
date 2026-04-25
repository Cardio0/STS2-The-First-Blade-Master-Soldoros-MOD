using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Powers;

// 류심-극 파워 — 무색 카드로 적을 공격할 때마다 해당 적의 힘을 이번 턴 동안 Amount만큼 감소.
// MonarchsGazePower 패턴 + IsColorless 필터.
public sealed class FlowingStanceNeoPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
    };

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult _, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != base.Owner) return;
        if (!props.IsPoweredAttack()) return;
        if (cardSource?.VisualCardPool.IsColorless != true) return;

        await PowerCmd.Apply<FlowingStanceNeoStrengthDownPower>(target, base.Amount, base.Owner, null);
    }
}
