using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 무장 전용: 방어도 = 유물 수.
// BaseValue = 0, UpdateCardPreview 에서 유물 수를 PreviewValue 에 합산.
// → 유물이 1개 이상이면 diff() 가 초록색으로 표기됨.
public class RelicCountBlockVar : BlockVar
{
    public RelicCountBlockVar() : base(0m, ValueProp.Move) { }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
        PreviewValue += card.Owner?.Relics.Count ?? 0;
    }
}
