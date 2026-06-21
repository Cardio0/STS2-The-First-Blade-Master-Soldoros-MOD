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
using Soldoros.SoldorosCode.Extensions;

namespace Soldoros.SoldorosCode.Powers;

// 류심 진 힘 감소 파워 — 이번 턴 동안 적 힘 N 감소. 적 턴 종료 시 복원.
// SoldorosPower 직접 구현 — TemporaryStrengthPower 미사용 (영문 하드코딩 + 카드 호버 문제)
public sealed class FlowingStanceNeoStrengthDownPower : SoldorosPower
{
    // flowing_stance_neo_strength_down_power.png 가 없으므로 류심 진 파워 아이콘 공유
    public override string CustomPackedIconPath => "flowing_stance_neo_power.png".PowerImagePath();
    public override string CustomBigIconPath => "flowing_stance_neo_power.png".BigPowerImagePath();

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
    };

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), target, -amount, applier, cardSource, silent: true);
    }

    // 스택 추가 시(BeforeApplied 이후) 힘 감소 델타 적용
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;
        if (amount == base.Amount) return; // 최초 적용은 BeforeApplied에서 처리
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -amount, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy) return;
        Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, base.Amount, base.Owner, null);
    }
}
