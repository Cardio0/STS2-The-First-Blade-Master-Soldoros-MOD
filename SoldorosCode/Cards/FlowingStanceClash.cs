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
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Cards;

// 류심 충 — 류심 토큰 (공격).
// FlowingStance 카드가 손에 생성. 휘발성(Ethereal) + 소멸(Exhaust).
// 꿰뚫기: AsheFork 와 동일한 패턴 — 취약 스택 저장 → 제거 → base×(1+stacks) 피해.
public sealed class FlowingStanceClash : SoldorosTokenCard, IFlowingStanceCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Ethereal,
        CardKeyword.Exhaust,
    };

    // 취약 적이 있으면 카드 금색 강조
    protected override bool ShouldGlowGoldInternal =>
        base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<VulnerablePower>()) ?? false;

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
        new PierceDamageVar(3m),
    };

    public FlowingStanceClash() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        int stacks = cardPlay.Target.GetPower<VulnerablePower>()?.Amount ?? 0;

        if (stacks > 0)
            await PowerCmd.Remove<VulnerablePower>(cardPlay.Target);

        decimal finalDamage = base.DynamicVars.Damage.BaseValue * (1 + stacks);
        await DamageCmd.Attack(finalDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
