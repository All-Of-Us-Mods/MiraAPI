using HarmonyLib;
using Il2CppSystem;
using MiraAPI.GameOptions;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

/*
[HarmonyPatch(typeof(GameOptionsMenu))]
public static class GameOptionsMenuPatch
{
    /// <summary>
    /// Change the OnValueChanged action for all custom game options
    /// </summary>
    [HarmonyPostfix, HarmonyPatch(nameof(GameOptionsMenu.Start))]
    public static void StartPostfix(GameOptionsMenu __instance)
    {
        foreach (var customOption in ModdedOptionsManager.Options)
        {
            if (customOption.AdvancedRole is not null || !customOption.OptionBehaviour)
            {
                continue;
            }

            customOption.OptionBehaviour.OnValueChanged = (Action<OptionBehaviour>)customOption.ValueChanged;
        }
    }
    

    /// <summary>
    /// Set the position and offset of all custom game options
    /// </summary>
    [HarmonyPostfix, HarmonyPatch(nameof(GameOptionsMenu.Update))]
    public static void UpdatePostfix(GameOptionsMenu __instance)
    {
        if (!GameSettingMenu.Instance)
        {
            return;
        }

        if (GameSettingMenu.Instance.GameSettingsTab.gameObject.active || GameSettingMenu.Instance.RoleSettingsTab.gameObject.active)
        {
            return;
        }

        
        var startOffset = 2.5f;
        __instance.GetComponentInParent<Scroller>().ContentYBounds.max = startOffset + __instance.Children.Count * 0.5f;

        foreach (var option in ModdedOptionsManager.Options.Where(option => option.Group == null))
        {
            if (!option.OptionBehaviour) continue;

            option.OptionBehaviour.enabled = option.Visible();
            option.OptionBehaviour.gameObject.SetActive(option.Visible());

            if (option.Visible())
            {
                startOffset -= 0.55f;
            }

            var transform = option.OptionBehaviour.transform;
            var optionPosition = transform.localPosition;
            transform.localPosition = new Vector3(optionPosition.x, startOffset, optionPosition.z);
        }

        foreach (var group in ModdedOptionsManager.Groups.Where(group => group.AdvancedRole == null))
        {
            if (group.Header == null)
            {
                continue;
            }

            group.Header.SetActive(group.GroupVisible());

            if (group.GroupVisible())
            {
                startOffset -= 0.5f;
            }

            var position = group.Header.transform.localPosition;
            group.Header.transform.localPosition = new Vector3(position.x, startOffset, position.z);

            foreach (var option in ModdedOptionsManager.Options.Where(x => x.Group == group))
            {
                if (!option.OptionBehaviour)
                {
                    continue;
                }

                var enabled = group.GroupVisible() && option.Visible();

                option.OptionBehaviour.enabled = enabled;
                option.OptionBehaviour.gameObject.SetActive(enabled);

                if (group.GroupVisible() && option.Visible())
                {
                    startOffset -= 0.5f;
                }

                var transform = option.OptionBehaviour.transform;
                var optionPosition = transform.localPosition;
                transform.localPosition = new Vector3(optionPosition.x, startOffset, optionPosition.z);
            }
        }
    }
}*/