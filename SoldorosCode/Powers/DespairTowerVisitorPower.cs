using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Powers;

// 절망의 탑에서 온 자 파워 — 다른 플레이어가 취약을 부여할 때마다 힘 1 획득.
public sealed class DespairTowerVisitorPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    };

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power is not VulnerablePower) return;
        if (amount <= 0) return;
        if (applier == null || !applier.IsPlayer) return;
        if (applier == base.Owner) return;   // 자신이 부여한 취약 제외

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, 1, base.Owner, null);
    }
}
