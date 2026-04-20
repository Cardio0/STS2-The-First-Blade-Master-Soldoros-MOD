using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Cards;

// 신기의 손놀림 — 고급 스킬. 0 코스트.
// 카드 1장 뽑기. 무색 공격이면 이번 전투 동안 피해 +1(→+2).
public sealed class DeftGrip : SoldorosCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Bonus", 1m),
    };

    public DeftGrip() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? drawn = (await CardPileCmd.Draw(choiceContext, 1m, base.Owner)).FirstOrDefault();
        if (drawn != null && drawn.Type == CardType.Attack && drawn.VisualCardPool.IsColorless)
        {
            decimal bonus = base.DynamicVars["Bonus"].BaseValue;
            foreach (DynamicVar v in drawn.DynamicVars.Values)
            {
                if (v is DamageVar damageVar)
                    damageVar.UpgradeValueBy(bonus);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Bonus"].UpgradeValueBy(1m);   // 1 → 2
    }
}
