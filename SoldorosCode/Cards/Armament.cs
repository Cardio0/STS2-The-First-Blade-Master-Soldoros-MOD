using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Soldoros.SoldorosCode.Cards;

public sealed class Armament : SoldorosCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    // RelicCountBlockVar: 카드 미리보기에 유물 수를 반영 (초록색 수치 표기).
    // CalculatedVar "CalculatedBlock": 인게임 전투 중 실제 방어도 수치를 괄호 안에 표시.
    //   공식 = CalculationBase(0) + CalculationExtra(1) × relicCount = relicCount
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new RelicCountBlockVar(),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedBlock").WithMultiplier((card, _) => card.Owner?.Relics.Count ?? 0),
    };

    public Armament() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        base.DynamicVars["Block"].BaseValue = base.Owner.Relics.Count;
        await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
