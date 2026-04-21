using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Cards;

public sealed class AsheFork : SoldorosCard
{
    // 취약 상태인 적이 있으면 금색 테두리 강조
    protected override bool ShouldGlowGoldInternal =>
        base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<VulnerablePower>()) ?? false;

    // 꿰뚫기 키워드 툴팁
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return new HoverTip(
                new LocString("card_keywords", "SOLDOROS-PIERCE.title"),
                new LocString("card_keywords", "SOLDOROS-PIERCE.description")
            );
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PierceDamageVar(3m),   // 기본 3, 강화 시 5
    };

    public AsheFork() : base(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 1. 대상의 취약 수치 저장
        int stacks = cardPlay.Target.GetPower<VulnerablePower>()?.Amount ?? 0;

        // 2. 취약 전부 제거 (1.5× 훅이 피해 계산에 개입하지 않도록)
        if (stacks > 0)
        {
            await PowerCmd.Remove<VulnerablePower>(cardPlay.Target);
        }

        // 3. 꿰뚫기 피해 적용 (힘 보정 포함)
        // 엔진 결과: finalDamage + str = (base+str)×(1+stacks)
        int str = base.Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0;
        decimal finalDamage = base.DynamicVars.Damage.BaseValue * (1 + stacks) + (decimal)str * stacks;
        await DamageCmd.Attack(finalDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(2m);   // 3 → 5
    }
}
