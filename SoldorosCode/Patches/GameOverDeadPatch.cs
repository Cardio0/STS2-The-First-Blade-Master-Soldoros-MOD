using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// 사망 시 게임오버 화면에서 솔도로스 시체 이미지로 교체한다.
///
/// 원인: 모드 DLL 배포 시 Godot이 .tscn 스크립트(SNCreatureVisuals 등)를
///       인스턴스화하지 못해 BaseLib가 기본 클래스(NCreatureVisuals)로 자동 변환.
///       따라서 커스텀 타입 대신 기본 클래스로 체크하고
///       텍스처 경로에 "soldoros"가 포함된 경우에만 교체한다.
///
/// 케이스:
///   1. 전투 사망     → NCreatureVisuals → Sprite(Sprite2D)
///   2. 상점 사망     → NMerchantCharacter → Sprite(Sprite2D)
///   3. 휴식처 사망   → NRestSiteCharacter → Sprite(Sprite2D)
/// </summary>
[HarmonyPatch(typeof(NGameOverScreen), nameof(NGameOverScreen.AfterOverlayOpened))]
public static class GameOverDeadPatch
{
    private const string CorpsePath = "res://Soldoros/images/character/soldoros_corpse.png";

    [HarmonyPostfix]
    private static void SwapCorpseTexture(NGameOverScreen __instance)
    {
        var corpseTexture = GD.Load<Texture2D>(CorpsePath);
        if (corpseTexture == null) return;

        // %CreatureContainer 먼저 시도, 없으면 전체 씬 탐색
        Node? searchRoot = __instance.GetNodeOrNull<Node>("%CreatureContainer") ?? __instance;
        SwapInTree(searchRoot, corpseTexture);
    }

    private static void SwapInTree(Node node, Texture2D corpseTexture)
    {
        foreach (var child in node.GetChildren())
        {
            bool handled = false;

            // 상점 케이스: NMerchantCharacter → Sprite(Sprite2D)
            if (child is NMerchantCharacter merchantChar)
            {
                var sp = merchantChar.GetNodeOrNull<Sprite2D>("Sprite");
                if (sp != null && IsSoldorosTexture(sp.Texture))
                    SwapCorpseSprite(sp, corpseTexture);
                handled = true;
            }
            // 휴식처 케이스: NRestSiteCharacter → Sprite(Sprite2D)
            else if (child is NRestSiteCharacter restSiteChar)
            {
                var sp = restSiteChar.FindChild("Sprite") as Sprite2D;
                if (sp != null && IsSoldorosTexture(sp.Texture))
                    SwapCorpseSprite(sp, corpseTexture);
                handled = true;
            }
            // 전투/일반 케이스: NCreatureVisuals → Visuals/Sprite(Sprite2D)
            // 기본 게임 캐릭터는 "Sprite" Sprite2D 노드가 없으므로 자연 필터링됨
            else if (child is NCreatureVisuals creatureVisuals)
            {
                var sp = creatureVisuals.FindChild("Sprite") as Sprite2D;
                if (sp != null && IsSoldorosTexture(sp.Texture))
                    SwapCorpseSprite(sp, corpseTexture);
                handled = true;
            }

            if (!handled)
                SwapInTree(child, corpseTexture);
        }
    }

    /// <summary>
    /// 시체 텍스처로 교체하고 스케일·위치를 조정한다.
    /// corpse(2000px) vs normal(1024px) 비율 보정 후 2/3 추가 축소 → 0.15.
    /// GlobalPosition.X 기준으로 전투(좌측)/비전투(중앙~우측)를 구분하여 위치 오프셋 적용.
    /// 기존 Scale.X 부호를 유지하여 좌우 반전 상태를 보존한다.
    /// </summary>
    private static void SwapCorpseSprite(Sprite2D sprite, Texture2D corpseTexture)
    {
        // GlobalPosition은 Position 수정 전에 읽어야 함
        float globalX = sprite.GlobalPosition.X;

        sprite.Texture = corpseTexture;
        float xSign = sprite.Scale.X < 0 ? -1f : 1f;
        sprite.Scale = new Vector2(xSign * 0.15f, 0.15f);

        // threshold는 로그 확인 후 교정 필요
        // 전투 사망 = 화면 좌측(globalX 작음), 비전투 사망 = 중앙~우측(globalX 큼)
        const float CombatThreshold = 500f;
        bool isOutOfCombat = globalX > CombatThreshold;
        sprite.Position = isOutOfCombat
            ? new Vector2(0f, -150f)    // 비전투: 조정값 (threshold 교정 후 수정)
            : new Vector2(150f, -150f); // 전투: 기존 오프셋
    }

    /// <summary>
    /// 텍스처 경로에 "soldoros"가 포함되어 있으면 솔도로스 캐릭터로 판별.
    /// </summary>
    private static bool IsSoldorosTexture(Texture2D? texture)
        => texture?.ResourcePath?.Contains("soldoros") == true;
}
