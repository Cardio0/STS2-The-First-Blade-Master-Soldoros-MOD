using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Soldoros.SoldorosCode.Cards;

namespace Soldoros.SoldorosCode.Powers;

// 자아를 가진 검 파워 — 에고소드 클라리스를 뽑을 때마다 방어도 Amount 획득.
public sealed class SentientBladePower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Block),
    };

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner != base.Owner.Player) return;
        if (card is not EgoswordClararis) return;

        Flash();
        await CreatureCmd.GainBlock(base.Owner, base.Amount, ValueProp.Unpowered, null);
    }
}
