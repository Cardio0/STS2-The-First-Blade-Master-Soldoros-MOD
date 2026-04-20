using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 절제된 공격 — 피해 후 무작위 류심 토큰(승/퀘/충)을 손에 생성.
public sealed class MeasuredStrike : SoldorosCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<FlowingStanceRise>(),
        HoverTipFactory.FromCard<FlowingStanceSwift>(),
        HoverTipFactory.FromCard<FlowingStanceClash>(),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move),
    };

    public MeasuredStrike() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        if (base.CombatState is null) return;

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        CardModel token = CreateRandomFlowingStanceToken();
        await CardPileCmd.AddGeneratedCardsToCombat(
            new[] { token }, PileType.Hand, addedByPlayer: true);
    }

    // Rng.Shuffle 을 사용해 0~2 무작위 선택 → 류심 승/퀘/충 중 하나 생성
    private CardModel CreateRandomFlowingStanceToken()
    {
        int roll = base.Owner.RunState.Rng.Shuffle.NextInt(3);
        return roll switch
        {
            0 => base.CombatState!.CreateCard<FlowingStanceRise>(base.Owner),
            1 => base.CombatState!.CreateCard<FlowingStanceSwift>(base.Owner),
            _ => base.CombatState!.CreateCard<FlowingStanceClash>(base.Owner),
        };
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(3m);   // 6 → 9
    }
}
