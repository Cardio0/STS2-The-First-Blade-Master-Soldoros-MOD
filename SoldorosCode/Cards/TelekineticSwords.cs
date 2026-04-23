using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Soldoros.SoldorosCode.Powers;

namespace Soldoros.SoldorosCode.Cards;

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
