using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Soldoros.SoldorosCode.Relics;

// 웨펀셀렉터 — 일반. 7번의 전투가 끝날 때마다 무작위 유물을 1개 얻습니다.
public sealed class WeaponSelector : SoldorosRelic
{
    private int _combatsWon;

    public override RelicRarity Rarity => RelicRarity.Common;
    public override bool ShowCounter => true;
    public override int DisplayAmount => CombatsWon % 7;

    [SavedProperty]
    public int CombatsWon
    {
        get => _combatsWon;
        private set
        {
            AssertMutable();
            _combatsWon = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterCombatVictoryEarly(CombatRoom room)
    {
        CombatsWon++;
        if (CombatsWon % 7 == 0)
        {
            Flash();
            RelicModel relic = RelicFactory.PullNextRelicFromFront(base.Owner);
            if (relic != null)
                await RelicCmd.Obtain(relic.ToMutable(), base.Owner);
        }
    }
}
