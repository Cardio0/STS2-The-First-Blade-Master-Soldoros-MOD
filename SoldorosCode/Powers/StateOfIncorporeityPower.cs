using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Soldoros.SoldorosCode.Cards;

namespace Soldoros.SoldorosCode.Powers;

// 무형의 경지 파워 — 류심 공격 카드 사용 시 에너지 1 획득.
public sealed class StateOfIncorporeityPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player) return;
        if (cardPlay.Card is not IFlowingStanceCard) return;
        if (cardPlay.Card.Type != CardType.Attack) return;

        Flash();
        await PlayerCmd.GainEnergy((decimal)base.Amount, base.Owner.Player);
    }
}
