using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// Clare… — 에고소드 클라리스를 어디서든 손으로 가져오고 방어도 획득.
// 뽑을/버린/소멸 더미를 모두 탐색; 없으면 새로 생성.
public sealed class ClarisCall : SoldorosCard
{
    // 호버 시 에고소드 클라리스 카드 미리보기 표시
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<EgoswordClararis>(),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(8m, ValueProp.Move),
    };

    public ClarisCall() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<EgoswordClararis> allClarisCards = base.Owner.PlayerCombatState?.AllCards
            .OfType<EgoswordClararis>()
            .ToList() ?? new List<EgoswordClararis>();

        // 이미 손에 있으면 불발 (유일 키워드 — 대령하라와 동일한 동작)
        bool alreadyInHand = allClarisCards.Any(c => c.Pile?.Type == PileType.Hand);

        if (!alreadyInHand)
        {
            List<EgoswordClararis> inOtherPiles = allClarisCards
                .Where(c => c.Pile?.Type != PileType.Hand)
                .ToList();

            if (inOtherPiles.Count > 0)
            {
                // 뽑기/버리기/소멸 더미에 있으면 손으로 이동
                foreach (EgoswordClararis card in inOtherPiles)
                    await CardPileCmd.Add(card, PileType.Hand);
            }
            else if (base.CombatState != null)
            {
                // 어디에도 없을 때만 새로 생성
                EgoswordClararis newCard = base.CombatState.CreateCard<EgoswordClararis>(base.Owner);
                await CardPileCmd.AddGeneratedCardsToCombat(
                    new List<CardModel> { newCard }, PileType.Hand, cardPlay.Card.Owner);
            }
        }

        // 방어도는 항상 획득 (손에 이미 있어도)
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(4m);   // 8 → 12
    }
}
