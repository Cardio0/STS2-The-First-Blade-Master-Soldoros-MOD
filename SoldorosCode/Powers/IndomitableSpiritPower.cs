using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Powers;

// 불굴의 의지 파워 — 이번 턴 동안 잃는 체력이 10을 넘을 수 없음.
// 고동치는 잔여물(BeatingRemnant) 패턴을 파워로 구현.
// 적 턴 종료 시 TickDown 제거 (AutoGuardPower 패턴).
public sealed class IndomitableSpiritPower : SoldorosPower
{
    private const decimal HpCap = 10m;

    // 이번 턴 동안 실제로 잃은 체력 누적
    private decimal _hpLostThisTurn = 0m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner) return amount;
        decimal remaining = HpCap - _hpLostThisTurn;
        return Math.Max(0m, Math.Min(amount, remaining));
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner) return Task.CompletedTask;
        if (result.WasFullyBlocked) return Task.CompletedTask;
        _hpLostThisTurn += (decimal)result.UnblockedDamage;
        return Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Enemy)
        {
            _hpLostThisTurn = 0m;
            await PowerCmd.TickDownDuration(this);
        }
    }
}
