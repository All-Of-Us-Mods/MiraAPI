using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example;

[RegisterModifier]
public class ModifierExample : TimedModifier
{
    public override string ModifierName => "Timed modifier example";
    public override float Duration => 1.5f;
    public override bool AutoStart => true;

    private System.Random _rand = new();

    public override void OnTimerComplete()
    {
        Player.SetColor((byte)_rand.Next(0, 18));
        StartTimer();
    }
}