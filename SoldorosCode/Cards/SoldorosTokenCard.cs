using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using Soldoros.SoldorosCode.Extensions;

namespace Soldoros.SoldorosCode.Cards;

// 류심 토큰 카드 전용 베이스 클래스.
// EgoswordClararis 와 동일하게 TokenCardPool 에 등록.
// → 솔도로스 캐릭터 풀 제외, 백과사전 토큰 섹션에 표시, STS004 경고 없음.
[Pool(typeof(TokenCardPool))]
public abstract class SoldorosTokenCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
}
