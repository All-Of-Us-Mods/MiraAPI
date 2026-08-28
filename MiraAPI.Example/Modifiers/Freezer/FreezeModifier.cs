using Il2CppSystem;
using MiraAPI.Example.Roles;
using MiraAPI.Modifiers.Types;
using MiraAPI.Translation;
using UnityEngine;

namespace MiraAPI.Example.Modifiers.Freezer;

public class FreezeModifier : TimedModifier
{
    public override string ModifierName => MiraLocaleManager.Get("modifier.freezed");
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

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Player.AmOwner || PlayerControl.LocalPlayer.Data.Role is FreezerRole)
        {
            Player.cosmetics.SetOutline(true, new Nullable<Color>(Palette.LightBlue));
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
