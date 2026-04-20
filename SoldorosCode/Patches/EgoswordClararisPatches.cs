using System;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using Soldoros.SoldorosCode.Cards;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// 유일 키워드 — 변화 차단 (1/2).
/// IsTransformable = false → 원시의 힘 등 IsTransformable 필터를 쓰는
/// 변화 효과의 대상 목록에서 자동으로 제외됨.
/// </summary>
[HarmonyPatch(typeof(CardModel), "get_IsTransformable")]
static class PatchEgoswordClararisTransformable
{
    [HarmonyPostfix]
    static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__instance is EgoswordClararis)
            __result = false;
    }
}

/// <summary>
/// 유일 키워드 — 변화 차단 (2/2).
/// 막아라, 사라져라 등 filter:null로 선택 후 CardCmd.Transform을 직접 호출하는
/// 카드가 에고소드 클라라리스에 변화를 시도하면 예외 없이 조용히 무시.
/// </summary>
[HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Transform),
    new[] { typeof(CardModel), typeof(CardModel), typeof(CardPreviewStyle) })]
static class PatchTransformSkipClararis
{
    [HarmonyPrefix]
    static bool Prefix(CardModel original, ref Task<CardPileAddResult?> __result)
    {
        if (!original.IsTransformable)
        {
            __result = Task.FromResult<CardPileAddResult?>(null);
            return false;
        }
        return true;
    }
}

/// <summary>
/// 유일 키워드 — 복사 차단 (이도류 / 악몽 / 선조의 망치).
/// 위 카드들의 손패 선택 목록에서 에고소드 클라리스를 제외.
/// CardSelectCmd.FromHand의 filter를 해당 source일 때만 추가 제한.
/// </summary>
[HarmonyPatch(typeof(CardSelectCmd), "FromHand")]
static class PatchFromHandExcludeClararis
{
    [HarmonyPrefix]
    static void Prefix(AbstractModel source, ref Func<CardModel, bool>? filter)
    {
        if (source is DualWield or Nightmare or HeirloomHammer)
        {
            var original = filter;
            filter = c => c is not EgoswordClararis && (original == null || original(c));
        }
    }
}

/// <summary>
/// 유일 키워드 — 복사 차단 (저글링 카운팅).
/// 저글링 파워의 공격 카드 카운터에서 에고소드 클라라리스를 제외.
/// 에고소드가 3번째 공격 카드로 사용되어도 복제 발동 카운트에 포함하지 않음.
/// </summary>
[HarmonyPatch(typeof(JugglingPower), nameof(JugglingPower.AfterCardPlayed))]
static class PatchJugglingSkipClararis
{
    [HarmonyPrefix]
    static bool Prefix(CardPlay cardPlay, ref Task __result)
    {
        if (cardPlay.Card is EgoswordClararis)
        {
            __result = Task.CompletedTask;
            return false;
        }
        return true;
    }
}
