using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Relics;

// 할기의 본링 — 상점. 공격할 의도가 있는 적 상대로 사용하는 공격 카드의 피해량이 25% 증가합니다.
public sealed class HalgisBoneRing : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack()) return 1m;
        if (target == null || dealer != base.Owner.Creature) return 1m;
        if (cardSource?.Type != CardType.Attack) return 1m;
        if (target.Monster?.IntendsToAttack == true) return 1.25m;
        return 1m;
    }
}
