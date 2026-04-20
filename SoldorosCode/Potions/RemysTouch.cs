using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Soldoros.SoldorosCode.Potions;

// 레미의 손길 — 일반. 최대 체력의 10% 회복 + 에너지 1 획득.
public sealed class RemysTouch : SoldorosPotion
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.AnyTime;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("HealPercent", 10m),
        new EnergyVar(1),
    };

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, Colors.Red);

        await CreatureCmd.Heal(target, (decimal)target.MaxHp * base.DynamicVars["HealPercent"].BaseValue / 100m);

        if (NCombatRoom.Instance != null && target.Player != null)
            await PlayerCmd.GainEnergy(base.DynamicVars.Energy.BaseValue, target.Player);
    }
}
