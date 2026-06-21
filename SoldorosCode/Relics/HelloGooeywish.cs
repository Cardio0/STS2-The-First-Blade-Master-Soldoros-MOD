using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Relics;

// 헬로 구위시 — 고급. 약화를 부여할 때마다, 취약을 부여합니다.
public sealed class HelloGuish : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not WeakPower) return;
        if (applier != base.Owner.Creature) return;
        if (amount <= 0) return;
        if (power.Owner.Side == base.Owner.Creature.Side) return;

        Flash();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, power.Owner, amount, base.Owner.Creature, cardSource);
    }
}
