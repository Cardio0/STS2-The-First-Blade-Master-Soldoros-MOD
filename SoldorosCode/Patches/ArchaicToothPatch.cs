using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using Soldoros.SoldorosCode.Cards;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// ArchaicTooth(고대의 이빨) — 솔도로스 Transcendence 연동.
///
/// TranscendenceUpgrades(private static getter)에
/// 에쉔 포크(AsheFork) → 천제극섬(Pentastrike) 매핑을 추가한다.
///
/// 연쇄 효과:
///   TranscendenceCards = TranscendenceUpgrades.Values 이므로
///   Pentastrike가 자동으로 TranscendenceCards에 포함됨.
///   → DustyTome(먼지 쌓인 책)이 Ancient 카드 중 TranscendenceCards를 제외하고
///     무형지경(StateOfIncorporeity)을 보상으로 제공.
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
public static class ArchaicToothPatch
{
    [HarmonyPostfix]
    private static void AddSoldorosMapping(ref Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<AsheFork>().Id] = ModelDb.Card<Pentastrike>();
    }
}
