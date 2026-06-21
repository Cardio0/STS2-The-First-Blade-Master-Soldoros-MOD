using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Soldoros.SoldorosCode.Extensions;

namespace Soldoros.SoldorosCode.Cards;

[Pool(typeof(TokenCardPool))]
public sealed class EgoswordClararis : CustomCardModel
{
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    // 공명 키워드 텍스트를 {ResonanceKeyword} 플레이스홀더로 주입
    // {IfUpgraded:show:} 대신 이 방식을 쓰는 이유:
    //   업그레이드 미리보기는 별도의 IsUpgraded=true 카드 객체를 생성하므로
    //   {IfUpgraded:show:}의 [green] 래퍼 없이 항상 [gold] 그대로 출력됨
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        string keywordText = "";
        if (base.IsUpgraded)
        {
            string title = new LocString("card_keywords", "SOLDOROS-RESONANCE.title").GetFormattedText();
            keywordText = "\n[gold]" + title + "[/gold].";
        }
        description.Add("ResonanceKeyword", keywordText);
    }

    // 유일 키워드 툴팁: card_keywords.json의 SOLDOROS-UNIQUE 항목을 LocString으로 직접 참조
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return new HoverTip(
                new LocString("card_keywords", "SOLDOROS-UNIQUE.title"),
                new LocString("card_keywords", "SOLDOROS-UNIQUE.description")
            );
            if (base.IsUpgraded)
            {
                yield return new HoverTip(
                    new LocString("card_keywords", "SOLDOROS-RESONANCE.title"),
                    new LocString("card_keywords", "SOLDOROS-RESONANCE.description")
                );
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move),                          // OnPlay 피해 — 힘·약화·취약 적용
        new DamageVar("DrawDamage", 6m, ValueProp.Unpowered),       // 뽑기 피해 — 고정 수치, 항상 기본값으로 표시
    };

    // 유일 키워드: 무작위 무색 카드 생성 풀에서 제외 (기쁨의 선물 등)
    public override bool CanBeGeneratedByModifiers => false;

    public EgoswordClararis()
        : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 업그레이드 효과: 사용할 때마다 이번 전투 동안 피해 2배
        // Token 카드이므로 전투 종료 시 카드가 사라져 리셋 불필요
        if (base.IsUpgraded)
        {
            base.DynamicVars.Damage.BaseValue *= 2;
            base.DynamicVars["DrawDamage"].BaseValue *= 2;
        }
    }

    // 유일 키워드: 플레이 후 소멸이 아닌 버리기 더미로
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
        => card == this ? (PileType.Discard, position) : (pileType, position);

    // 유일 키워드: 복사본이 어느 더미에 추가되든 즉시 전투에서 완전 제거
    //
    // 흐름 구분:
    //   CreateCard (유물 등으로 정식 생성)
    //     → ToMutable() → MutableClone() → AfterCloned() [_isClone=true]
    //     → AfterCreated()                               [_isClone=false 로 복원] ← 원본
    //
    //   CreateClone / CreateDupe (이도류·저글링 복사)
    //     → ClonePreservingMutability() → MutableClone() → AfterCloned() [_isClone=true]
    //     → AfterCreated() 호출 없음                                      ← 복사본
    private bool _isClone;

    protected override void AfterCloned()
    {
        base.AfterCloned();
        _isClone = true;
    }

    // CreateCard 경로에서만 호출됨 → 정식 생성된 원본임을 표시
    public override void AfterCreated()
    {
        base.AfterCreated();
        _isClone = false;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (card != this) return;

        // 복사본 제거 로직 (유일 키워드)
        // card.Pile != null && IsCombatPile → 실제 더미에 있을 때만 제거
        // (RemoveFromCombat이 내부적으로 AfterCardChangedPiles를 재호출하므로
        //  두 번째 호출 시에는 card.Pile == null → 조건 불충족 → 재진입 방지)
        if (_isClone)
        {
            if (card.Pile != null && card.Pile.IsCombatPile)
                await CardPileCmd.RemoveFromCombat(card);
            return;
        }

        // 뽑을 카드 더미 → 손 이동 시 피해 발동
        // AfterCardDrawn(일반 뽑기)뿐 아니라 손으로 가져오기 효과도 모두 감지
        if (oldPileType == PileType.Draw && card.Pile?.Type == PileType.Hand)
        {
            if (base.CombatState is null) return;
            // Unpowered 크리처 딜 → 약화·취약 배율 무시, 가시 반사 없음
            await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), base.CombatState.HittableEnemies,
                base.DynamicVars["DrawDamage"].BaseValue, ValueProp.Unpowered, base.Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        // 기본 피해량은 6 그대로 유지
        // 업그레이드 효과: 사용할 때마다 피해 2배 (OnPlay에서 IsUpgraded로 처리)
    }
}
