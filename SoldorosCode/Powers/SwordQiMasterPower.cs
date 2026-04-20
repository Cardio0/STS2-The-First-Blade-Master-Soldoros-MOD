using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Soldoros.SoldorosCode.Cards;

namespace Soldoros.SoldorosCode.Powers;

// 검기상인 파워 — 에고소드 클라리스가 막히지 않은 피해를 줄 때마다 약화 1 부여.
public sealed class SwordQiMasterPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<WeakPower>(),
    };

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != base.Owner) return;
        if (cardSource is not EgoswordClararis) return;
        if (result.UnblockedDamage <= 0) return;

        Flash();
        await PowerCmd.Apply<WeakPower>(target, base.Amount, base.Owner, null);
    }
}
