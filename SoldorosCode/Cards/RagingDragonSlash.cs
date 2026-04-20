using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Cards;

public sealed class RagingDragonSlash : SoldorosCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Shuffle", 2m),   // 기본 2장, 강화 시 1장
    };

    public RagingDragonSlash() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 4, base.Owner);

        int shuffleCount = base.DynamicVars["Shuffle"].IntValue;
        var selected = await CardSelectCmd.FromHand(
            context: choiceContext,
            player: base.Owner,
            prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, shuffleCount),
            filter: null,
            source: this);

        // 선택한 카드를 전부 뽑을 카드 더미에 추가한 뒤 뽑을 카드 더미만 셔플.
        // CardPileCmd.Shuffle 은 버린 카드 더미까지 섞으므로 사용하지 않음.
        foreach (CardModel card in selected)
        {
            await CardPileCmd.Add(card, PileType.Draw);
        }

        if (selected.Any())
        {
            PileType.Draw.GetPile(base.Owner).RandomizeOrderInternal(
                base.Owner, base.Owner.RunState.Rng.Shuffle, base.CombatState!);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Shuffle"].UpgradeValueBy(-1m);   // 2 → 1
    }
}
