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

public sealed class FairAndSquare : SoldorosCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<PlatingPower>(),
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PlatingPower>(5m),
        new BlockVar(5m, ValueProp.Unpowered),
        new DynamicVar("EnemyPlating", 4m),
    };

    public override bool GainsBlock => false;

    public FairAndSquare() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState is null) return;

        await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, base.DynamicVars["PlatingPower"].BaseValue, base.Owner.Creature, this);
        foreach (Creature enemy in base.CombatState.HittableEnemies)
        {
            await CreatureCmd.GainBlock(enemy, base.DynamicVars.Block, cardPlay);
            await PowerCmd.Apply<PlatingPower>(choiceContext, enemy, base.DynamicVars["EnemyPlating"].BaseValue, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["PlatingPower"].UpgradeValueBy(2m);
        base.DynamicVars.Block.UpgradeValueBy(2m);
        base.DynamicVars["EnemyPlating"].UpgradeValueBy(2m);
    }
}
