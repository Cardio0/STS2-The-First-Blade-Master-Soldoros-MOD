using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Soldoros.SoldorosCode.Extensions;
using Soldoros.SoldorosCode.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using Soldoros.SoldorosCode.Cards;

namespace Soldoros.SoldorosCode.Character;

public class Soldoros : PlaceholderCharacterModel
{
    public const string CharacterId = "Soldoros";
    
    public static readonly Color Color = new("ffffff");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 77;
    
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeSoldoros>(),
        ModelDb.Card<StrikeSoldoros>(),
        ModelDb.Card<StrikeSoldoros>(),
        ModelDb.Card<StrikeSoldoros>(),
        ModelDb.Card<DefendSoldoros>(),
        ModelDb.Card<DefendSoldoros>(),
        ModelDb.Card<DefendSoldoros>(),
        ModelDb.Card<DefendSoldoros>(),
        ModelDb.Card<AsheFork>(),
        ModelDb.Card<UpwardSlash>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<EgoswordClararisRelic>()
    ];
    
    public override CardPoolModel CardPool => ModelDb.CardPool<SoldorosCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SoldorosRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SoldorosPotionPool>();
    
    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets. 
        These are just some of the simplest assets, given some placeholders to differentiate your character with. 
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    public override string CustomIconTexturePath => "character_icon_soldoros.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_soldoros.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_soldoros_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_soldoros.png".CharacterUiPath();

    // 정적 Sprite2D 기반 캐릭터 씬 (Spine 대체)
    public override string CustomVisualPath          => "res://Soldoros/scenes/soldoros_visual.tscn";
    public override string CustomMerchantAnimPath    => "res://Soldoros/scenes/soldoros_merchant.tscn";
    public override string CustomRestSiteAnimPath    => "res://Soldoros/scenes/soldoros_rest_site.tscn";
    public override string CustomIconPath            => "res://Soldoros/scenes/soldoros_icon.tscn";
    public override string CustomTrailPath           => "card_trail_soldoros.tscn".ScenePath();
    public override string CustomCharacterSelectBg   => "char_select/char_select_bg_soldoros.tscn".ScenePath();

    // 캐릭터 선택 시 재생되는 음성
    public override string CharacterSelectSfx => "res://Soldoros/audio/soldoros_select.ogg";

    // 전투 에너지 카운터 — 카드 비용 젬 이미지 재사용 (회전 레이어 2·3은 투명 처리)
    public override CustomEnergyCounter? CustomEnergyCounter =>
        new CustomEnergyCounter(
            i => $"res://Soldoros/images/ui/combat/energy_counters/soldoros_energy_layer_{i}.png",
            new Color("ffffff"),          // 숫자 텍스트 색 (게임 하드코딩 cream, 변경 불가)
            new Color("8B5010"));        // 숫자 외곽선 색 (갈색, 불투명)
}