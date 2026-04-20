using BaseLib.Abstracts;
using Soldoros.SoldorosCode.Extensions;
using Godot;

namespace Soldoros.SoldorosCode.Character;

public class SoldorosPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Soldoros.Color;
    

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}