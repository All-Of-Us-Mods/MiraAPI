using MiraAPI.Example.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Example.Modifiers;

public class CaptainModifier : GameModifier
{
    public override string ModifierName => MiraLocaleManager.Get("modifier.captain");
    public override LoadableAsset<Sprite>? ModifierIcon => ExampleAssets.CallMeetingButton;

    public override void OnDeath(DeathReason reason)
    {
        Player.RemoveModifier(this);
    }

    public override string GetDescription()
    {
        return MiraLocaleManager.Get("modifier.captain.TabDescription");
    }

    public override int GetAssignmentChance()
    {
        return (int)OptionGroupSingleton<CaptainModifierSettings>.Instance.Chance;
    }

    public override int GetAmountPerGame()
    {
        return (int)OptionGroupSingleton<CaptainModifierSettings>.Instance.Amount;
    }
}
