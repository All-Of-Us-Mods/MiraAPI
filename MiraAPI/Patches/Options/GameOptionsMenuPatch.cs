using HarmonyLib;
using Il2CppSystem;
using MiraAPI.API.GameOptions;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameOptionsMenu))]
public static class GameOptionsMenuPatch
{
    /// <summary>
    /// Change the OnValueChanged action for all custom game options
    /// </summary>
    [HarmonyPostfix, HarmonyPatch("Start")]
    public static void StartPostfix(GameOptionsMenu __instance)
    {
        foreach (var customOption in CustomOptionsManager.CustomOptions)
        {
            if (customOption.AdvancedRole is not null || !customOption.OptionBehaviour)
            {
                continue;
            }

            customOption.OptionBehaviour.OnValueChanged = (Action<OptionBehaviour>)customOption.ValueChanged;
        }
    }

    private static GameSettingMenu menu;


    /// <summary>
    /// Set the position and offset of all custom game options
    /// </summary>
    [HarmonyPostfix, HarmonyPatch("Update")]
    public static void UpdatePostfix(GameOptionsMenu __instance)
    {
        if (!menu)
        {
            menu = Object.FindObjectsOfType<GameSettingMenu>().First();
        }

        if (menu.RegularGameSettings.active || menu.RolesSettings.gameObject.active || menu.HideNSeekSettings.active)
        {
            return;
        }

        var startOffset = 2.15f;
        __instance.GetComponentInParent<Scroller>().ContentYBounds.max = startOffset + __instance.Children.Count * 0.5f;

        foreach (var option in CustomOptionsManager.CustomOptions.Where(option => option.Group == null))
        {
            if (!option.OptionBehaviour) continue;

            option.OptionBehaviour.enabled = !option.Hidden();
            option.OptionBehaviour.gameObject.SetActive(!option.Hidden());

            if (!option.Hidden())
            {
                startOffset -= 0.55f;
            }

            var transform = option.OptionBehaviour.transform;
            var optionPosition = transform.localPosition;
            transform.localPosition = new Vector3(optionPosition.x, startOffset, optionPosition.z);
        }

        foreach (var group in CustomOptionsManager.CustomGroups.Where(group => group.AdvancedRole == null))
        {
            if (group.Header == null)
            {
                continue;
            }

            group.Header.SetActive(!group.Hidden());

            if (!group.Hidden())
            {
                startOffset -= 0.5f;
            }

            var position = group.Header.transform.localPosition;
            group.Header.transform.localPosition = new Vector3(position.x, startOffset, position.z);

            foreach (var option in group.Options)
            {
                if (!option.OptionBehaviour)
                {
                    continue;
                }

                var enabled = !group.Hidden() && !option.Hidden();

                option.OptionBehaviour.enabled = enabled;
                option.OptionBehaviour.gameObject.SetActive(enabled);

                if (!group.Hidden() && !option.Hidden())
                {
                    startOffset -= 0.5f;
                }

                var transform = option.OptionBehaviour.transform;
                var optionPosition = transform.localPosition;
                transform.localPosition = new Vector3(optionPosition.x, startOffset, optionPosition.z);
            }
        }
    }
}