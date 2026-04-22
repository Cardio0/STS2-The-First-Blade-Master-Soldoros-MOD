using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Soldoros.SoldorosCode.Cards;

// 류심 — 류심 승/퀘/충 세 장을 손에 생성.
// 기본: 비용 2. 소멸 없음.
// 강화(류심+): 류심 승+/퀘+/충+ 생성. 설명 및 호버팁도 + 버전으로 표시.
public sealed class FlowingStance : SoldorosCard
{
    // 마우스 호버 시 류심 승/퀘/충(+) 카드 썸네일 표시
    // IsUpgraded 시 upgrade: true 를 전달하여 + 버전 호버팁을 보여줌
    protected override IEnumerable<IHoverTip> ExtraHoverTips => IsUpgraded
        ? new IHoverTip[]
          {
              HoverTipFactory.FromCard<FlowingStanceRise>(upgrade: true),
              HoverTipFactory.FromCard<FlowingStanceSwift>(upgrade: true),
              HoverTipFactory.FromCard<FlowingStanceClash>(upgrade: true),
          }
        : new IHoverTip[]
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

        // 류심+: 생성 토큰도 강화(+) 버전으로
        if (IsUpgraded)
            foreach (var t in tokens) t.UpgradeInternal();

        await CardPileCmd.AddGeneratedCardsToCombat(tokens, PileType.Hand, addedByPlayer: true);
    }
}
