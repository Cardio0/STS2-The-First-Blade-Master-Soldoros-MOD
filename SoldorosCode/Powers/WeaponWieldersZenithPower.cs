using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Powers;

// 무기수련의 극의 파워 — 이번 턴 무색 카드 사용 시 힘 2 획득(일시적).
// 턴 종료 시 자동 제거 + 획득한 힘 전부 회수.
public sealed class WeaponWieldersZenithPower : SoldorosPower
{
    private int _strengthApplied = 0;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
    };

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player) return;
        if (!cardPlay.Card.VisualCardPool.IsColorless) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(context, base.Owner, base.Amount, base.Owner, null, silent: true);
        _strengthApplied += base.Amount;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != base.Owner.Side) return;
        await PowerCmd.Remove(this);
        if (_strengthApplied > 0)
            await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner, -_strengthApplied, base.Owner, null, silent: true);
    }
}
