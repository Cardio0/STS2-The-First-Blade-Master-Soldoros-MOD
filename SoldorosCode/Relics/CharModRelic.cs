using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Soldoros.SoldorosCode.Character;
using Soldoros.SoldorosCode.Extensions;
using Godot;

namespace Soldoros.SoldorosCode.Relics;

[Pool(typeof(SoldorosRelicPool))]
public abstract class SoldorosRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
    protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();
    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}