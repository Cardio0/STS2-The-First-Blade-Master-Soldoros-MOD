using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Powers;

// 끝없는 수련 파워 — 다음 N턴 동안 턴 시작 시 힘 1 획득.
// Amount = 남은 턴 수. AfterEnergyReset 마다 Strength +1 → Decrement.
public sealed class EndlessTrainingPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
    };

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != base.Owner.Player) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(base.Owner, 1m, base.Owner, null);
        await PowerCmd.Decrement(this);
    }
}
