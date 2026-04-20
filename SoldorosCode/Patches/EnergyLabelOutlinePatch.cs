using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// NEnergyCounter.RefreshLabel()에서 아웃라인 색을 적용한 뒤,
/// 솔도로스 캐릭터일 경우 라벨 아웃라인 두께를 얇게 덮어씁니다.
/// </summary>
[HarmonyPatch(typeof(NEnergyCounter), "RefreshLabel")]
public static class EnergyLabelOutlinePatch
{
    private static readonly FieldInfo? PlayerField =
        typeof(NEnergyCounter).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);

    [HarmonyPostfix]
    private static void ThinOutline(NEnergyCounter __instance)
    {
        var player = PlayerField?.GetValue(__instance) as Player;
        if (player?.Character is not SoldorosCode.Character.Soldoros) return;

        var label = __instance.GetNodeOrNull<Label>("Label");
        if (label == null) return;

        // 기본 두께(보통 8~12)를 2로 줄여서 얇은 테두리 적용
        label.AddThemeConstantOverride("outline_size", 2);
    }
}
