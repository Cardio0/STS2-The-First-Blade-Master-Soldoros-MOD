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
using MegaCrit.Sts2.Core.ValueProps;

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

        // 3. 꿰뚫기 피해 적용 (힘·활력·인챈트 보정 포함)
        // 엔진 파이프라인: rawDamage → ①+ench → ②×corr → ③+str+vigor → ④×vuln(취약 제거됨)
        // 목표: ((base + ench) × corr + str + vigor) × (1 + stacks)
        // rawDamage 역산: base×(1+stacks) + ench×stacks + (str+vigor)×stacks/corr
        int str = base.Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0;
        int vigor = base.Owner.Creature.GetPower<VigorPower>()?.Amount ?? 0;
        decimal ench = this.Enchantment?.EnchantDamageAdditive(base.DynamicVars.Damage.BaseValue, ValueProp.Move) ?? 0m;
        decimal corr = this.Enchantment?.EnchantDamageMultiplicative(base.DynamicVars.Damage.BaseValue, ValueProp.Move) ?? 1m;
        decimal finalDamage = base.DynamicVars.Damage.BaseValue * (1 + stacks)
                            + ench * stacks
                            + (decimal)(str + vigor) * stacks / corr;
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
