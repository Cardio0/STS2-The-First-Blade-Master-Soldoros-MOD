using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Powers;

// 컨버전 파워 — 공격 카드로 적 방어도를 제거한 만큼 내 방어도 획득.
public sealed class ConversionPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Block),
    };

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer != base.Owner) return;
        if (!props.IsPoweredAttack()) return;
        if (cardSource?.Type != CardType.Attack) return;
        if (result.BlockedDamage <= 0) return;

        Flash();
        await CreatureCmd.GainBlock(base.Owner, result.BlockedDamage, ValueProp.Unpowered, null, fast: true);
    }
}
