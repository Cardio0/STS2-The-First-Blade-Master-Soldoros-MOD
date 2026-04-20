using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 극 귀검술: 폭풍식 — 희귀 공격. 2 코스트.
// 피해 5(→7). 이번 전투에서 사용한 무색 카드 수만큼 반복. 인게임에 총 횟수 표시.
public sealed class UltimateSlayTempest : SoldorosCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new CalculationBaseVar(0m),   // 반복 횟수 기저 0 (기본 타격은 별도)
        new CalculationExtraVar(1m),  // 무색 카드 1장당 반복 +1회
        new CalculatedVar("CalculatedHits").WithMultiplier(
            static (CardModel card, Creature? _) =>
                CombatManager.Instance.History.CardPlaysFinished.Count(
                    (CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == card.Owner && e.CardPlay.Card.VisualCardPool.IsColorless)),
    };

    public UltimateSlayTempest() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int colorlessCount = CombatManager.Instance.History.CardPlaysFinished.Count(
            (CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == base.Owner && e.CardPlay.Card.VisualCardPool.IsColorless);
        int hitCount = 1 + colorlessCount;

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(hitCount)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);   // 5 → 7
    }
}
