using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 오버드라이브 — 고급 스킬. 체력 3 손실. 대상 취약 × 3(→4)배. 소멸.
public sealed class Overdrive : SoldorosCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[]
    {
        CardKeyword.Exhaust,
    };

    protected override bool ShouldGlowGoldInternal =>
        base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<VulnerablePower>()) ?? false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<VulnerablePower>(),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Multiplier", 3m),
    };

    public Overdrive() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        // 체력 3 손실 (막기 불가, 파워 미적용)
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 3m,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, null, this);

        // 취약 × Multiplier 배
        int current = cardPlay.Target.GetPowerAmount<VulnerablePower>();
        if (current > 0)
        {
            int multiplier = (int)base.DynamicVars["Multiplier"].BaseValue;
            int add = current * (multiplier - 1);
            await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, add, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Multiplier"].UpgradeValueBy(1m);   // 3 → 4
    }
}
