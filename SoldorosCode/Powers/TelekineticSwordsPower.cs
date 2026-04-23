using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Soldoros.SoldorosCode.Powers;

// 이기어검술 파워 — 이번 턴 공격 카드 사용 시 무작위 무색 카드 1장 획득.
// 플레이어 턴 종료 시 자동 제거. SpectrumShiftPower + JugglingPower 패턴.
public sealed class TelekineticSwordsPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player) return;
        if (cardPlay.Card.Type != CardType.Attack) return;

        Flash();
        Player player = base.Owner.Player;
        var cards = CardFactory.GetDistinctForCombat(
            player,
            ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint),
            base.Amount,
            player.RunState.Rng.CombatCardGeneration);

        foreach (var card in cards)
        {
            card.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == base.Owner.Side)
            await PowerCmd.Remove(this);
    }
}
