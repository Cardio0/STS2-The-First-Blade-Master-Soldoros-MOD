using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 꿰뚫기 카드 전용 DamageVar:
// 대상의 취약 수치를 읽어 엔진 파이프라인 결과 × (1 + stacks) 를 미리보기로 표시.
// 취약 1.5× 배율 대신 stacks 수에 비례한 pierce 배율 사용.
// 훅 실행 후 PreviewValue = (base + ench) × corr + str + vigor 이므로
// × (1 + stacks) 만 추가하면 올바른 미리보기가 됨.
public class PierceDamageVar : DamageVar
{
    public PierceDamageVar(decimal damage) : base(damage, ValueProp.Move) { }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        int stacks = target?.GetPower<VulnerablePower>()?.Amount ?? 0;

        // 취약이 있을 때: null 타겟으로 훅 실행해 1.5× 취약 배율 제외.
        // 취약이 없을 때: 실제 타겟으로 정상 계산.
        base.UpdateCardPreview(card, previewMode, stacks > 0 ? null : target, runGlobalHooks);

        if (stacks > 0)
        {
            // 훅 결과 PreviewValue = (base + ench) × corr + str + vigor
            // pierce 배율 적용: × (1 + stacks)
            PreviewValue *= (1 + stacks);
        }
    }
}
