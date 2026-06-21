using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace Soldoros.SoldorosCode.Relics;

// 솔도로스의 선택 — 희귀. 매 전투 시작 시, 보유한 유물 5개마다 힘을 1 얻습니다.
public sealed class SoldorosDecision : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom) return;

        int strength = base.Owner.Relics.Count / 5;
        if (strength <= 0) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), base.Owner.Creature, strength, null, null);
    }
}
