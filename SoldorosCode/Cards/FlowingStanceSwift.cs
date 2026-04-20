using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 류심 퀘 — 류심 토큰 (공격).
// FlowingStance 카드가 손에 생성. 휘발성(Ethereal) + 소멸(Exhaust).
public sealed class FlowingStanceSwift : SoldorosTokenCard, IFlowingStanceCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Ethereal,
        CardKeyword.Exhaust,
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
    };

    public FlowingStanceSwift() : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(2)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
