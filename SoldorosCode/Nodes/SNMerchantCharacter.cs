using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Soldoros.SoldorosCode.Nodes;

/// <summary>
/// 솔도로스 상점 캐릭터 노드.
/// 원본 _Ready()는 GetChild(0)을 Spine 스프라이트로 가정하므로 호출을 막고,
/// 대신 정적 TextureRect(SpineSprite)를 씬에서 직접 사용한다.
/// </summary>
[GlobalClass]
public partial class SNMerchantCharacter : NMerchantCharacter
{
    public override void _Ready()
    {
        // base._Ready() 호출 안 함 — 원본이 Spine을 전제하므로 스킵
    }
}
