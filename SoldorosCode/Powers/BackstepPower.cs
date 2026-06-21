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

// 백스텝 파워 — 이번 턴 동안 민첩 N 부여. 플레이어 턴 종료 시 민첩 회수.
// SoldorosPower 직접 구현 — TemporaryDexterityPower 미사용 (영문 하드코딩 + 카드 호버 문제)
public sealed class BackstepPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DexterityPower>(),
    };

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<DexterityPower>(new BlockingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);
    }

    // 스택 추가 시 민첩 델타 적용
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;
        if (amount == base.Amount) return;
        await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner, amount, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, base.Owner, -base.Amount, base.Owner, null);
    }
}
