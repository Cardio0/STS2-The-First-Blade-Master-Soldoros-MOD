using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Powers;

// 극 귀검술: 심검 파워 — 다음 N턴 동안 플레이어 턴 시작 시 DoubleDamage 부여.
// Amount = 남은 턴 수. 매 턴 시작 시 TickDown, 0이 되면 자동 제거.
public sealed class OmnislayMindsSwordPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DoubleDamagePower>(),
    };

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            Flash();
            await PowerCmd.Apply<DoubleDamagePower>(new BlockingPlayerChoiceContext(), base.Owner, 1, base.Owner, null);
            await PowerCmd.TickDownDuration(this);
        }
    }
}
