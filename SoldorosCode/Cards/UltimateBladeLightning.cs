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

// 극 발검술: 섬단 — 희귀 공격. 1 코스트.
// 피해 10 + 이번 전투에서 사용한 무색 카드 수 × 1. 단일 타격. 업그레이드: 잔류.
// 덱 UI / 카드 보상: 기본값 10 표시. 전투 중: 현재 계산값 표시.
public sealed class UltimateBladeLightning : SoldorosCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(10m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(
            static (CardModel card, Creature? _) =>
                card.Owner.PlayerCombatState?.AllCards.Contains(card) == true ?
                CombatManager.Instance?.History?.CardPlaysFinished?.Count(
                    (CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == card.Owner && e.CardPlay.Card.VisualCardPool.IsColorless) ?? 0 : 0),
    };

    public UltimateBladeLightning() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
