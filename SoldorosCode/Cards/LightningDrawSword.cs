using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 극초발도 — 희귀 공격. 2 코스트. 전체 대상.
// 피해 = (10 + 유물 수) × STR/버프/취약 배율.
// CalculatedDamageVar 패턴: 유물 수가 base에 flat 합산된 뒤 모든 배율 적용.
// → 덱 UI: 유물 수 0 → 10 표시 / 전투 중: (10+유물수) 기반 정확한 미리보기.
public sealed class LightningDrawSword : SoldorosCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(10m),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(
            static (CardModel card, Creature? _) =>
                card.Owner.PlayerCombatState?.AllCards.Contains(card) == true
                    ? card.Owner.Relics.Count : 0),
    };

    public LightningDrawSword() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState is null) return;

        decimal totalDamage = base.DynamicVars["CalculationBase"].BaseValue + base.Owner.Relics.Count;
        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .TargetingAllOpponents(base.CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["CalculationBase"].UpgradeValueBy(5m);   // 10 → 15
    }
}
