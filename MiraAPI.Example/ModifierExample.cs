using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example;

[RegisterModifier]
public class ModifierExample : TimedModifier
{
    public override string ModifierName => "Timed modifier example";
    public override float Duration => 10;

    public override void OnTimerComplete()
    {

    }
}