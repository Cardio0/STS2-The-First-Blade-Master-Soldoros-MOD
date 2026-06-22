using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Soldoros.SoldorosCode.Cards;

namespace Soldoros.SoldorosCode.Relics;

public sealed class AwakenedEgoswordClararisRelic : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<EgoswordClararis>(upgrade: true)
    ];

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner || player.PlayerCombatState!.TurnNumber > 1) return;

        Flash();
        CardModel card = combatState.CreateCard<EgoswordClararis>(base.Owner);
        CardCmd.Upgrade(card, CardPreviewStyle.None);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner);
    }

    public override Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
    {
        if (shuffler != base.Owner) return Task.CompletedTask;

        CardModel? card = base.Owner.PlayerCombatState?.DrawPile.Cards
            .FirstOrDefault(c => c is EgoswordClararis);

        if (card != null)
            base.Owner.PlayerCombatState!.DrawPile.MoveToTopInternal(card);

        return Task.CompletedTask;
    }
}
