using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Relics;
using SoldorosCharacter = Soldoros.SoldorosCode.Character.Soldoros;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// TouchOfOrobas(오르바스의 손) — 솔도로스 미등장 처리.
///
/// Orobas 이벤트는 SetupForPlayer()가 true를 반환해야 선택지에 추가됨.
/// 솔도로스의 시작 유물(EgoswordClararisRelic)은 Starter 등급이라
/// 기본 로직에서 true를 반환하고, 취득 시 Circlet으로 교체되는 문제가 발생.
///
/// 솔도로스 캐릭터일 때 SetupForPlayer()를 false로 단락시켜
/// 이벤트 선택지 자체를 차단한다.
/// </summary>
[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.SetupForPlayer))]
public static class TouchOfOrobasPatch
{
    [HarmonyPrefix]
    private static bool BlockForSoldoros(Player player, ref bool __result)
    {
        if (player.Character is SoldorosCharacter)
        {
            __result = false;
            return false; // 원본 메서드 실행 생략
        }
        return true; // 다른 캐릭터는 원본 실행
    }
}
