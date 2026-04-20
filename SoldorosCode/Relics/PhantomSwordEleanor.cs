using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Relics;

// 무형검 엘레노어 — 희귀. 방어도를 보유한 적에게 사용하는 공격 카드의 피해량이 2배로 증가합니다.
public sealed class ShapelessSwordElenore : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack()) return 1m;
        if (target == null || dealer != base.Owner.Creature) return 1m;
        if (cardSource?.Type != CardType.Attack) return 1m;
        if (target.Block > 0) return 2m;
        return 1m;
    }
}
