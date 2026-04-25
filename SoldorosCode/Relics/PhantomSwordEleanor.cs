using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Relics;

// 무형검 엘레노어 — 희귀. 방어도를 보유한 적에게 사용하는 공격 카드의 피해량이 2배로 증가합니다.
// 카드 시전 시점의 방어도를 기준으로 판단 — 다타 공격 중 방어도가 깨져도 전체 타격에 2배 적용.
public sealed class ShapelessSwordElenore : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    // 카드 시전 시점 각 적의 방어도 보유 여부를 캐싱
    private readonly Dictionary<Creature, bool> _hadBlock = new();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        _hadBlock.Clear();
        if (cardPlay.Card.Owner != base.Owner) return Task.CompletedTask;
        if (cardPlay.Card.Type != CardType.Attack) return Task.CompletedTask;

        // 공격 카드 시전 직전 — 모든 적의 방어도 보유 여부를 스냅샷
        foreach (Creature enemy in base.Owner.Creature.CombatState?.HittableEnemies ?? [])
        {
            _hadBlock[enemy] = enemy.Block > 0;
        }
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack()) return 1m;
        if (target == null || dealer != base.Owner.Creature) return 1m;
        if (cardSource?.Type != CardType.Attack) return 1m;
        // 실제 시전 중: BeforeCardPlayed 스냅샷 기준 (다타 도중 방어도가 깨져도 전타 2배 유지)
        // 미리보기 중: 스냅샷이 비어있으므로 현재 방어도 직접 체크로 폴백
        if (_hadBlock.TryGetValue(target, out bool hadBlock)) return hadBlock ? 2m : 1m;
        return target.Block > 0 ? 2m : 1m;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        _hadBlock.Clear();
        return Task.CompletedTask;
    }
}
