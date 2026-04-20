using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Soldoros.SoldorosCode.Cards;

namespace Soldoros.SoldorosCode.Potions;

// 단진의 항아리 — 고급. 류심 승/퀘/충을 손으로 가져옵니다.
public sealed class DanjinsPot : SoldorosPotion
{
    public override PotionRarity Rarity => PotionRarity.Uncommon;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.AnyPlayer;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        AssertValidForTargetedPotion(target);
        NCombatRoom.Instance?.PlaySplashVfx(target, Colors.Green);

        await CardPileCmd.AddToCombatAndPreview<FlowingStanceRise>(base.Owner.Creature, PileType.Hand, 1, true, CardPilePosition.Top);
        await CardPileCmd.AddToCombatAndPreview<FlowingStanceSwift>(base.Owner.Creature, PileType.Hand, 1, true, CardPilePosition.Top);
        await CardPileCmd.AddToCombatAndPreview<FlowingStanceClash>(base.Owner.Creature, PileType.Hand, 1, true, CardPilePosition.Top);
    }
}
