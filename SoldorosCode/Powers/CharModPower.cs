using BaseLib.Abstracts;
using BaseLib.Extensions;
using Soldoros.SoldorosCode.Extensions;
using Godot;

namespace Soldoros.SoldorosCode.Powers;

public abstract class SoldorosPower : CustomPowerModel
{
    //Loads from Soldoros/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}