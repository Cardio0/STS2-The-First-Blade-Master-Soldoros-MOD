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

// 극 발검술: 섬단 — 희귀 공격. 2 코스트.
// 피해 10 고정 타격 후, 이번 전투에서 사용한 무색 카드 수만큼 추가 피해. 업그레이드: 잔류.
public sealed class UltimateBladeLightning : SoldorosCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),     // 기본 타격 10 (표기용)
        new CalculationBaseVar(0m),             // CalculatedDamage 기저값 0
        new ExtraDamageVar(1m),                 // 무색 카드 1장당 +1 피해
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(
            static (CardModel card, Creature? _) =>
                CombatManager.Instance.History.CardPlaysFinished.Count(
                    (CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == card.Owner && e.CardPlay.Card.VisualCardPool.IsColorless)),
        // CalculatedDamage = 0 + 1 × N = N (무색 카드 수)
    };

    public UltimateBladeLightning() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 1타: 기본 10 피해
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 2타: 무색 카드 수만큼 추가 피해
        decimal extraDamage = (decimal)CombatManager.Instance.History.CardPlaysFinished.Count(
            (CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == base.Owner && e.CardPlay.Card.VisualCardPool.IsColorless);
        if (extraDamage > 0)
            await DamageCmd.Attack(extraDamage)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
