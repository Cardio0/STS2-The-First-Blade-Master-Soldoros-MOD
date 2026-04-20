using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Soldoros.SoldorosCode.Cards;

public sealed class DecisiveStrike : SoldorosCard
{
    // 취약 상태인 적이 있으면 금색 테두리 강조
    protected override bool ShouldGlowGoldInternal =>
        base.CombatState?.HittableEnemies.Any((Creature e) => e.HasPower<VulnerablePower>()) ?? false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get { yield return new HoverTip(
            new MegaCrit.Sts2.Core.Localization.LocString("card_keywords", "SOLDOROS-PIERCE.title"),
            new MegaCrit.Sts2.Core.Localization.LocString("card_keywords", "SOLDOROS-PIERCE.description")); }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PierceDamageVar(16m),
    };

    public DecisiveStrike() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        int stacks = cardPlay.Target.GetPower<VulnerablePower>()?.Amount ?? 0;
        if (stacks > 0)
            await PowerCmd.Remove<VulnerablePower>(cardPlay.Target);
        decimal finalDamage = base.DynamicVars.Damage.BaseValue * (1 + stacks);
        await DamageCmd.Attack(finalDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(4m);   // 16 → 20
    }
}
