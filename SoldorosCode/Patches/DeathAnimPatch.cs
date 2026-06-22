using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using SoldorosCharacter = Soldoros.SoldorosCode.Character.Soldoros;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// NCreature.StartDeathAnim()을 패치하여 Spine 사망 애니메이션 대신
/// 솔도로스 전용 시체 이미지로 교체한다.
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.StartDeathAnim))]
public static class DeathAnimPatch
{
    [HarmonyPostfix]
    private static void SwapToCorpseSprite(NCreature __instance)
    {
        // 솔도로스 캐릭터가 아니면 무시
        if (__instance.Entity?.Player?.Character is not SoldorosCharacter)
            return;

        var sprite = __instance.Visuals?.FindChild("Sprite") as Sprite2D;
        if (sprite == null)
            return;

        var corpseTexture = GD.Load<Texture2D>("res://Soldoros/images/character/soldoros_corpse.png");
        if (corpseTexture == null)
            return;

        sprite.Texture = corpseTexture;
        float xSign = sprite.Scale.X < 0 ? -1f : 1f;
        sprite.Scale = new Vector2(xSign * 0.15f, 0.15f);
        sprite.Position = new Vector2(-20f, -120f);
    }
}
