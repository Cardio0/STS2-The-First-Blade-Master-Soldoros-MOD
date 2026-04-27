using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace Soldoros.SoldorosCode.Patches;

// 캐릭터 선택창에서 솔도로스 제목이 패널 밖으로 넘치는 문제 수정.
// SelectCharacter Postfix로 Name 라벨의 폰트 크기를 솔도로스일 때만 축소.
[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
internal static class CharacterSelectTitleSizePatch
{
    private static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
    {
        if (characterModel is not global::Soldoros.SoldorosCode.Character.Soldoros) return;

        var nameLabel = __instance.GetNodeOrNull<Label>("InfoPanel/VBoxContainer/Name");
        if (nameLabel == null) return;

        nameLabel.AddThemeFontSizeOverride("font_size", 33);
    }
}
