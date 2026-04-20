using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Soldoros.SoldorosCode.Cards;

// 꿰뚫기 카드 전용 DamageVar:
// 대상의 취약 수치를 읽어 base × (1 + stacks) 를 미리보기 피해로 표시.
// 취약 보정치(1.5×) 대신 pierce 배율로 계산하므로 null 타겟으로 기본 훅 실행.
public class PierceDamageVar : DamageVar
{
    public PierceDamageVar(decimal damage) : base(damage, ValueProp.Move) { }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        int stacks = target?.GetPower<VulnerablePower>()?.Amount ?? 0;

        // 취약이 있을 때: null 타겟으로 훅 실행해 1.5× 취약 배율 제외,
        // 이후 pierce 배율(1 + stacks)을 수동으로 곱함.
        // 취약이 없을 때: 실제 타겟으로 정상 계산.
        base.UpdateCardPreview(card, previewMode, stacks > 0 ? null : target, runGlobalHooks);

        if (stacks > 0)
        {
            PreviewValue *= (1 + stacks);
        }
    }
}
