using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Cards;

// 류심 극 — 고급 스킬. 3 코스트.
// 소멸된 카드 더미에 있는 모든 류심 공격 카드를 손으로 가져옵니다. 업그레이드: 코스트 3→2.
public sealed class FlowingStanceZenith : SoldorosCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<FlowingStanceRise>(),
        HoverTipFactory.FromCard<FlowingStanceSwift>(),
        HoverTipFactory.FromCard<FlowingStanceClash>(),
    };

    public FlowingStanceZenith() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IEnumerable<CardModel> flowingCards = PileType.Exhaust.GetPile(base.Owner).Cards
            .Where(c => c is IFlowingStanceCard && c.Type == CardType.Attack)
            .ToList();

        foreach (CardModel card in flowingCards)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);   // 3 → 2
    }
}
