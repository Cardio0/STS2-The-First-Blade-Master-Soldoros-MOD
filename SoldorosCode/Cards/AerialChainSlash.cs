using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 공중 연속 베기 — 고급 공격. X 코스트.
// 피해 6만큼 X번. 약화 X 부여.
// 강화: X → X+1 (Malaise 패턴).
public sealed class AerialChainSlash : SoldorosCard
{
    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<WeakPower>(),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move),
    };

    public AerialChainSlash() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        int xValue = ResolveEnergyXValue();
        if (base.IsUpgraded) xValue++;

        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(xValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, xValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade() { }  // X → X+1 (IsUpgraded 플래그로 처리, 스탯 변경 없음)
}
