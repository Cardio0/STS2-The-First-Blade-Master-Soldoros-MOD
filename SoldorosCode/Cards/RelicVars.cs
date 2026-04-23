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

// 극초발도 전용: 기본 피해 + 유물 수 보너스 피해.
// base.UpdateCardPreview 가 BaseValue 를 재설정할 수 있으므로 직접 수정 금지.
// 대신 base 호출 후 적용된 배율(PreviewValue / BaseValue)을 역산하여
// 유물 보너스에도 동일 배율을 곱해 합산.
//   e.g. 취약 없음: ratio=10/10=1.0 → +2×1.0=2 → 최종 12 (녹색)
//   e.g. 취약 1:  ratio=15/10=1.5 → +2×1.5=3 → 최종 18 (녹색)
public class RelicBonusDamageVar : DamageVar
{
    public RelicBonusDamageVar(decimal damage) : base(damage, ValueProp.Move) { }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);

        // 덱 UI / 카드 보상 화면에서는 유물 보너스 없이 기본값만 표시
        if (card.Owner?.PlayerCombatState?.AllCards.Contains(card) != true) return;

        int relicCount = card.Owner.Relics.Count;
        if (relicCount <= 0) return;

        // base 가 적용한 배율 역산 (취약, 약화 등 포함)
        decimal ratio = (BaseValue > 0m) ? PreviewValue / BaseValue : 1m;
        PreviewValue += relicCount * ratio;
    }
}
