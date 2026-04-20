using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using Soldoros.SoldorosCode.Powers;

namespace Soldoros.SoldorosCode.Cards;

// 극 귀검술:참철식 — 스킬. 이번 턴 동안 막히지 않은 공격 피해를 줄 때마다 취약 1 부여.
// 강화: 비용 1 → 0.
public sealed class UltimateSlayIronStrike : SoldorosCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<VulnerablePower>(),
    };

    public UltimateSlayIronStrike() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<UltimateSlayIronStrikePower>(base.Owner.Creature, 1, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);   // 1 → 0
    }
}
