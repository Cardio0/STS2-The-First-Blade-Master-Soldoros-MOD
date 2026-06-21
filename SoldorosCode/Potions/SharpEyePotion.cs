using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Soldoros.SoldorosCode.Potions;

// 샤프아이 포션 — 희귀. 모든 적이 보유한 취약이 3배로 증가합니다.
public sealed class SharpEyePotion : SoldorosPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, Colors.Purple);

        var combatState = CombatManager.Instance?.DebugOnlyGetState();
        if (combatState == null) return;

        IEnumerable<Creature> enemies = combatState.GetOpponentsOf(base.Owner.Creature)
            .Where(e => e.IsAlive);

        foreach (Creature enemy in enemies)
        {
            int stacks = enemy.GetPowerAmount<VulnerablePower>();
            if (stacks <= 0) continue;
            int add = stacks * 2; // 현재 × 3배 = 현재 + 현재 × 2
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, add, base.Owner.Creature, null);
        }
    }
}
