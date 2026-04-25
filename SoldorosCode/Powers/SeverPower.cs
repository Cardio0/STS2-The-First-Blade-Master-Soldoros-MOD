using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Powers;

// 절단 파워 — 이번 턴 동안 적 힘 N 감소. 적 턴 종료 시 힘 복원.
// SoldorosPower 직접 구현 — TemporaryStrengthPower 미사용 (영문 하드코딩 + 카드 호버 문제)
public sealed class SeverPower : SoldorosPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
    };

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<StrengthPower>(target, -amount, applier, cardSource, silent: true);
    }

    // 스택 추가 시 힘 감소 델타 적용
    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;
        if (amount == base.Amount) return;
        await PowerCmd.Apply<StrengthPower>(base.Owner, -amount, applier, cardSource, silent: true);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Enemy) return;
        Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<StrengthPower>(base.Owner, base.Amount, base.Owner, null);
    }
}
