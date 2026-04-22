using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// 사망/기권/도전이력 화면에서 캐릭터 이름을 짧게 표시하는 패치.
///
/// 문제:
///   run_history 로컬라이제이션은 {character} 변수를 사용하고,
///   AddDetailsTo()는 {character} = Title(.title 키 = 풀 타이틀) 로 등록한다.
///   캐릭터 선택창은 CharacterSelectTitle → .title 키를 직접 조회하므로 영향 없음.
///
/// 해결:
///   솔도로스 캐릭터에 한해 Postfix로 {character}를 TitleObject(.titleObject 키)로 덮어씀.
///   → 사망/기권 화면: "솔도로스는 지쳤습니다" (짧은 이름)
///   → 캐릭터 선택창: "퍼스트 웨펀마스터 솔도로스" (풀 타이틀, 그대로)
/// </summary>
[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.AddDetailsTo))]
internal static class CharacterDeathNamePatch
{
    [HarmonyPostfix]
    private static void UseShortNameForDeathMessages(CharacterModel __instance, LocString str)
    {
        if (__instance is not global::Soldoros.SoldorosCode.Character.Soldoros)
            return;

        // {character} 변수를 TitleObject(짧은 이름)로 덮어씀
        str.Add("character", __instance.TitleObject);
    }
}
