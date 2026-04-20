using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Cards;

public sealed class DriveSlash : SoldorosCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get { yield return base.EnergyHoverTip; }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1),
    };

    public DriveSlash() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, base.Owner);
        await CardPileCmd.Draw(choiceContext, 1, base.Owner);

        CardModel? selected = (await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Owner,
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),
            filter: null,
            source: this)).FirstOrDefault();

        if (selected != null)
            await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);   // 보존 추가
    }
}
