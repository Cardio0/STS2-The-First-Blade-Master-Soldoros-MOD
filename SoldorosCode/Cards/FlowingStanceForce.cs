using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Soldoros.SoldorosCode.Powers;

namespace Soldoros.SoldorosCode.Cards;

// 류심 강 — 고급 파워. 1 코스트.
// 무색 공격 카드가 피해를 {FlowingStanceForcePower}만큼 더 줍니다. 업그레이드: 스택 +1.
public sealed class FlowingStanceForce : SoldorosCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<FlowingStanceForcePower>(3m),
    };

    public FlowingStanceForce() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FlowingStanceForcePower>(choiceContext, base.Owner.Creature, base.DynamicVars["FlowingStanceForcePower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["FlowingStanceForcePower"].UpgradeValueBy(1m);   // 3 → 4
    }
}
