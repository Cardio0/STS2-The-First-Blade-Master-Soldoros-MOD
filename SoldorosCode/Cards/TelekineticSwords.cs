using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Soldoros.SoldorosCode.Powers;

namespace Soldoros.SoldorosCode.Cards;

// 이기어검술 — 희귀 스킬. 1 코스트. 소멸.
// 이번 턴 공격 카드 사용 시 무작위 무색 카드 1장 획득. 업그레이드: 비용 1→0.
public sealed class TelekineticSwords : SoldorosCard
{
    public override System.Collections.Generic.IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public TelekineticSwords() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TelekineticSwordsPower>(base.Owner.Creature, 1m, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);   // 1 → 0
    }
}
