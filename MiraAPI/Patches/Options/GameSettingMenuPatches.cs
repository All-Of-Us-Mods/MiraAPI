using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions.UI;
using UnityEngine;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameSettingMenu))]
public static class GameSettingMenuPatches
{

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    public static void StartPrefix(GameSettingMenu __instance)
    {
        __instance.MenuDescriptionText.transform.parent.gameObject.SetActive(false);
    }

    /*public static void CreateOptionsFor(GameSettingMenu __instance, ToggleOption togglePrefab, NumberOption numberPrefab, StringOption stringPrefab, Transform container,
        List<IModdedOption> options)
    {
        foreach (IModdedOption option in options)
        {
            if (option.AdvancedRole is not null || option.OptionBehaviour)
            {
                continue;
            }

            OptionBehaviour newOption = option.CreateOption(togglePrefab, numberPrefab, stringPrefab, container);
            __instance.AllItems = __instance.AllItems.AddItem(newOption.transform).ToArray();
        }
    }*/
}
