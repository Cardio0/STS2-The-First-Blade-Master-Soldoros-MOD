using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Relics;

// 힘센 난장이의 황금 망치 — 고급. 획득 시, 비용이 2 이상인 무작위 카드 3장을 강화합니다.
public sealed class GoldenHammer : SoldorosRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override bool HasUponPickupEffect => true;

    public override Task AfterObtained()
    {
        IEnumerable<CardModel> targets = PileType.Deck.GetPile(base.Owner).Cards
            .Where(c => c != null && c.IsUpgradable && c.EnergyCost.Canonical >= 2)
            .ToList()
            .StableShuffle(base.Owner.RunState.Rng.Niche)
            .Take(3);

        foreach (CardModel card in targets)
            CardCmd.Upgrade(card);

        return Task.CompletedTask;
    }
}
