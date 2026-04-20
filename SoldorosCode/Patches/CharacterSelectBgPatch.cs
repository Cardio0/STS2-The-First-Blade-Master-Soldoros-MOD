using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Soldoros.SoldorosCode.Patches;

// 캐릭터 선택창 배경을 솔도로스 전용 정적 이미지 씬으로 교체.
[HarmonyPatch(typeof(CharacterModel), "CharacterSelectBg", MethodType.Getter)]
internal static class CharacterSelectBgPatch
{
    private static void Postfix(CharacterModel __instance, ref string __result)
    {
        if (__instance is global::Soldoros.SoldorosCode.Character.Soldoros)
        {
            __result = "res://Soldoros/scenes/char_select/char_select_bg_soldoros.tscn";
        }
    }
}
