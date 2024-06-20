using HarmonyLib;
using MiraAPI.GameOptions;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using MiraAPI.GameOptions.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameSettingMenu))]
public static class GameSettingsMenuPatches
{
    private static GameObject _gameBtn;
    private static GameObject _roleBtn;

    [HarmonyPrefix, HarmonyPatch("Start")]
    public static void StartPrefix(GameSettingMenu __instance)
    {
        foreach (var tab in ModdedOptionsTab.CustomTabs) tab.gameObject.Destroy();
        foreach (var screen in ModdedOptionsTab.CustomScreens) screen.gameObject.Destroy();

        ModdedOptionsTab.CustomScreens.Clear();
        ModdedOptionsTab.CustomTabs.Clear();

        _gameBtn = __instance.transform.FindChild("Header/Tabs/GameTab").gameObject;
        _roleBtn = __instance.transform.FindChild("Header/Tabs/RoleTab").gameObject;

        var numberOpt = __instance.RegularGameSettings.GetComponentInChildren<NumberOption>();
        var toggleOpt = Object.FindObjectOfType<ToggleOption>();
        var stringOpt = __instance.RegularGameSettings.GetComponentInChildren<StringOption>();

        foreach (var pair in ModdedOptionsManager.RegisteredMods)
        {
            var container = ModdedOptionsTab.InitializeForMod(pair.Value, __instance).transform;

            foreach (var group in ModdedOptionsManager.Groups.Where(group => group.AdvancedRole == null && group.ParentMod == pair.Value))
            {
                group.Header = ModdedOptionsTab.CreateHeader(toggleOpt, container, group.GroupName, group.GroupColor);
                CreateOptionsFor(__instance, toggleOpt, numberOpt, stringOpt, container, ModdedOptionsManager.Options.Where(x => x.Group == group).ToList());
            }

            CreateOptionsFor(__instance, toggleOpt, numberOpt, stringOpt, container, ModdedOptionsManager.Options.Where(x => x.Group is null).ToList());
        }

        if (!numberOpt || !toggleOpt || !stringOpt)
        {
            Logger<MiraAPIPlugin>.Error("OPTION PREFABS MISSING");
        }
    }

    public static void CreateOptionsFor(GameSettingMenu __instance, ToggleOption togglePrefab, NumberOption numberPrefab, StringOption stringPrefab, Transform container,
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
    }
}
