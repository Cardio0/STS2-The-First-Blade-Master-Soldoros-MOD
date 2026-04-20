using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Soldoros.SoldorosCode.Character;
using Soldoros.SoldorosCode.Extensions;

namespace Soldoros.SoldorosCode.Potions;

[Pool(typeof(SoldorosPotionPool))]
public abstract class SoldorosPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();

    public override string? CustomPackedOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".PotionImagePath();
}