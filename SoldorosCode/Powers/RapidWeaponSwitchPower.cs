using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Powers;

// 신속한 무기 교체 파워 — 턴 시작 시 카드 1장 드로우 후 손에서 1장을 뽑을 카드 더미 맨 위에.
public sealed class RapidWeaponSwitchPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player) return;

        Flash();
        await CardPileCmd.Draw(choiceContext, base.Amount, base.Owner.Player);

        var selected = await CardSelectCmd.FromHand(
            choiceContext,
            base.Owner.Player,
            new CardSelectorPrefs(new LocString("cards", "SOLDOROS-RAPID_WEAPON_SWITCH.selectionScreenPrompt"), base.Amount),
            null,
            this
        );

        if (selected != null)
            foreach (var card in selected)
                await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
    }
}
