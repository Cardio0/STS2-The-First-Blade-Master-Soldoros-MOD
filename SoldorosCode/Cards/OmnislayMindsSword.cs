using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Soldoros.SoldorosCode.Powers;

namespace Soldoros.SoldorosCode.Cards;

// 극 귀검술: 심검 — 희귀 공격. 1 코스트.
// 피해 6. 손의 모든 카드를 뽑을 더미에 섞기. 다음 턴 공격 카드 피해 2배. 업그레이드: 비용 1→0.
public sealed class OmnislayMindsSword : SoldorosCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<DoubleDamagePower>(),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move),
    };

    public OmnislayMindsSword() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 손에 있는 모든 카드를 뽑을 카드 더미에 섞어 넣음
        List<CardModel> handCards = PileType.Hand.GetPile(base.Owner).Cards.ToList();
        foreach (CardModel card in handCards)
        {
            await CardPileCmd.Add(card, PileType.Draw);
        }
        if (handCards.Any())
        {
            PileType.Draw.GetPile(base.Owner).RandomizeOrderInternal(
                base.Owner, base.Owner.RunState.Rng.Shuffle, base.CombatState!);
        }

        await PowerCmd.Apply<OmnislayMindsSwordPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);   // 1 → 0
    }
}
