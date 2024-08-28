using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;

namespace MiraAPI.Example.Modifiers.Freezer;

[RegisterModifier]
public class FreezeModifier : TimedModifier
{
    public override string ModifierName => "Freezed";
    public override bool HideOnUi => false;
    public override float Duration => 15f;
    public override bool AutoStart => true;
    public override bool RemoveOnComplete => true;

    public override void OnActivate()
    {
        if (Player.AmOwner)
        {
            Player.moveable = false;
        }
    }

    public override void OnTimerComplete()
    {
        if (Player.AmOwner)
        {
            Player.moveable = true;
        }
    }
}
