using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Soldoros.SoldorosCode.Cards;

// 류심 — 류심 승/퀘/충 세 장을 손에 생성하고 소멸.
public sealed class FlowingStance : SoldorosCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    // 마우스 호버 시 류심 승/퀘/충 카드 썸네일 표시 (선제 타격의 단도 패턴)
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<FlowingStanceRise>(),
        HoverTipFactory.FromCard<FlowingStanceSwift>(),
        HoverTipFactory.FromCard<FlowingStanceClash>(),
    };

    public FlowingStance() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState is null) return;

        var tokens = new List<MegaCrit.Sts2.Core.Models.CardModel>
        {
            base.CombatState.CreateCard<FlowingStanceRise>(base.Owner),
            base.CombatState.CreateCard<FlowingStanceSwift>(base.Owner),
            base.CombatState.CreateCard<FlowingStanceClash>(base.Owner),
        };

        await CardPileCmd.AddGeneratedCardsToCombat(tokens, PileType.Hand, addedByPlayer: true);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
