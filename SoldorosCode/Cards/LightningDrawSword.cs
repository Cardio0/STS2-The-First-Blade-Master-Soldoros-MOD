using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Soldoros.SoldorosCode.Cards;

public sealed class LightningDrawSword : SoldorosCard
{
    // RelicBonusDamageVar.UpdateCardPreview 에서 유물 수를 미리보기에 합산.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new RelicBonusDamageVar(10m),
    };

    public LightningDrawSword() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.CombatState is null) return;

        decimal totalDamage = base.DynamicVars.Damage.BaseValue + base.Owner.Relics.Count;
        await DamageCmd.Attack(totalDamage)
            .FromCard(this)
            .TargetingAllOpponents(base.CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(5m);   // 10 → 15
    }
}
