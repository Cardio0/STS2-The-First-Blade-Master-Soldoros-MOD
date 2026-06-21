using System.Collections.Generic;
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

// 데몬 슬레이브 — 희귀. 매 전투 시작 시 체력을 1 잃습니다, 힘을 3 얻습니다.
public sealed class DemonSlave : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Creature.Side) return;
        if (combatState.RoundNumber != 1) return;

        Flash();
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 1m,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, 3m, null, null);
    }
}
