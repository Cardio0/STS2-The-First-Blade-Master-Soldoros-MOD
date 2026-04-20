using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Relics;

// 데몬 슬레이브 — 일반. 매 전투 시작 시 체력을 2 잃습니다, 힘을 2 얻습니다.
public sealed class DemonSlave : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side != base.Owner.Creature.Side) return;
        if (combatState.RoundNumber != 1) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 2m,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        await PowerCmd.Apply<StrengthPower>(base.Owner.Creature, 2m, null, null);
    }
}
