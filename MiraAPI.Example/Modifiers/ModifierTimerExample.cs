using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example.Modifiers;

[RegisterModifier]
public class ModifierTimerExample : TimedModifier
{
    public override string ModifierName => "Color Changer";
    public override bool HideOnUi => true;
    public override float Duration => 10f;
    public override bool AutoStart => true;

    private System.Random _rand = new();

    public override void OnTimerComplete()
    {
        Player.SetColor((byte)_rand.Next(0, Palette.PlayerColors.Count));
        StartTimer();
    }
}
