using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Powers;

// 백어택 파워 — 카드 형태의 무형검 엘레노어.
// 방어도를 보유한 적에게 사용하는 공격 카드의 피해량이 Amount배.
// 중첩: 첫 사용 2배, 이후 사용마다 +1배. (Tracking 패턴)
// 스냅샷: BeforeCardPlayed에서 캡처, 다타 도중 방어도 격파 시에도 전타 배율 유지.
public sealed class RearAttackPower : SoldorosPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Block),
    };

    // 카드 시전 시점 각 적의 방어도 보유 여부를 캐싱 (엘레노어와 동일한 구조)
    private readonly Dictionary<Creature, bool> _hadBlock = new();

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        _hadBlock.Clear();
        if (cardPlay.Card.Owner != base.Owner.Player) return Task.CompletedTask;
        if (cardPlay.Card.Type != CardType.Attack) return Task.CompletedTask;

        foreach (Creature enemy in base.Owner.CombatState?.HittableEnemies ?? [])
        {
            _hadBlock[enemy] = enemy.Block > 0;
        }
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!props.IsPoweredAttack()) return 1m;
        if (target == null || dealer != base.Owner) return 1m;
        if (cardSource?.Type != CardType.Attack) return 1m;
        // 실제 시전 중: 스냅샷 기준
        // 미리보기 중: 스냅샷 비어있으므로 현재 방어도 직접 체크로 폴백
        if (_hadBlock.TryGetValue(target, out bool hadBlock)) return hadBlock ? base.Amount : 1m;
        return target.Block > 0 ? base.Amount : 1m;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        _hadBlock.Clear();
        return Task.CompletedTask;
    }
}
