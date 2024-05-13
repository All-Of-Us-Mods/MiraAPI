using HarmonyLib;
using MiraAPI.API.GameOptions;
using MiraAPI.GameModes;
using Reactor.Utilities;
using System.Collections.Generic;
using System.Linq;
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
        if (CustomOptionsTab.CustomTab)
        {
            return;
        }

        __instance.Tabs.transform.position += new Vector3(0.5f, 0, 0);
        _gameBtn = __instance.transform.FindChild("Header/Tabs/GameTab").gameObject;
        _roleBtn = __instance.transform.FindChild("Header/Tabs/RoleTab").gameObject;

        var numberOpt = __instance.RegularGameSettings.GetComponentInChildren<NumberOption>();
        var toggleOpt = Object.FindObjectOfType<ToggleOption>();
        var stringOpt = __instance.RegularGameSettings.GetComponentInChildren<StringOption>();
        var container = GameManager.Instance.IsNormal() ? CustomOptionsTab.Initialize(__instance).transform : __instance.HideAndSeekScroller.Inner;

        foreach (var group in CustomOptionsManager.CustomGroups.Where(group => group.AdvancedRole == null))
        {
            if (GameManager.Instance.IsNormal())
            {
                group.Header = CustomOptionsTab.CreateHeader(toggleOpt, container, group.Title);
                CreateOptionsFor(__instance, toggleOpt, numberOpt, stringOpt, container,
                    group.CustomToggleOptions, group.CustomNumberOptions, group.CustomStringOptions);
                continue;
            }

            if (!group.Options.Any(x => x.ShowInHideNSeek))
            {
                continue;
            }

            group.Header = CustomOptionsTab.CreateHeader(toggleOpt, container, group.Title);
            __instance.AllHideAndSeekItems = __instance.AllHideAndSeekItems.Append(group.Header.transform).ToArray();

            CreateOptionsFor(__instance, toggleOpt, numberOpt, stringOpt, container,
                group.CustomToggleOptions, group.CustomNumberOptions, group.CustomStringOptions);
        }

        CreateOptionsFor(__instance, toggleOpt, numberOpt, stringOpt, container,
            CustomOptionsManager.CustomToggleOptions.Where(option => option.Group == null),
            CustomOptionsManager.CustomNumberOptions.Where(option => option.Group == null),
            CustomOptionsManager.CustomStringOptions.Where(option => option.Group == null));

        if (!numberOpt || !toggleOpt || !stringOpt)
        {
            Logger<MiraAPIPlugin>.Error("OPTION PREFABS MISSING");
        }
    }

    [HarmonyPostfix, HarmonyPatch("Update")]
    public static void UpdatePatch(GameSettingMenu __instance)
    {
        if (CustomGameModeManager.ActiveMode == null)
        {
            return;
        }

        _gameBtn.SetActive(CustomGameModeManager.ActiveMode.CanAccessSettingsTab());
        _roleBtn.SetActive(CustomGameModeManager.ActiveMode.CanAccessRolesTab());

        if (!CustomGameModeManager.ActiveMode.CanAccessSettingsTab())
        {
            __instance.RegularGameSettings.SetActive(false);
            __instance.RolesSettings.gameObject.SetActive(false);

            CustomOptionsTab.CustomScreen.gameObject.SetActive(true);
            CustomOptionsTab.Rend.enabled = true;

            __instance.GameSettingsHightlight.enabled = false;
            __instance.RolesSettingsHightlight.enabled = false;
        }
    }

    public static void CreateOptionsFor(GameSettingMenu __instance, ToggleOption togglePrefab, NumberOption numberPrefab, StringOption stringPrefab, Transform container,
        IEnumerable<CustomToggleOption> toggles, IEnumerable<CustomNumberOption> numbers, IEnumerable<CustomStringOption> strings)
    {
        foreach (var customToggleOption in toggles)
        {
            if (customToggleOption.AdvancedRole is not null || customToggleOption.OptionBehaviour)
            {
                continue;
            }

            ToggleOption toggleOption;

            if (GameManager.Instance.IsNormal())
            {
                toggleOption = customToggleOption.CreateToggleOption(togglePrefab, container);
                __instance.AllItems = __instance.AllItems.AddItem(toggleOption.transform).ToArray();
                continue;
            }

            if (!customToggleOption.ShowInHideNSeek)
            {
                continue;
            }

            toggleOption = customToggleOption.CreateToggleOption(togglePrefab, container);
            __instance.AllHideAndSeekItems = __instance.AllHideAndSeekItems.AddItem(toggleOption.transform).ToArray();

        }

        foreach (var customNumberOption in numbers)
        {
            if (customNumberOption.AdvancedRole is not null || customNumberOption.OptionBehaviour)
            {
                continue;
            }

            NumberOption numberOption;

            if (GameManager.Instance.IsNormal())
            {
                numberOption = customNumberOption.CreateNumberOption(numberPrefab, container);
                __instance.AllItems = __instance.AllItems.AddItem(numberOption.transform).ToArray();
                continue;
            }

            if (!customNumberOption.ShowInHideNSeek)
            {
                continue;
            }

            numberOption = customNumberOption.CreateNumberOption(numberPrefab, container);
            __instance.AllHideAndSeekItems = __instance.AllHideAndSeekItems.AddItem(numberOption.transform).ToArray();
        }

        foreach (var customStringOption in strings)
        {
            if (customStringOption.AdvancedRole is not null || customStringOption.OptionBehaviour)
            {
                continue;
            }

            StringOption stringOption;

            if (GameManager.Instance.IsNormal())
            {
                stringOption = customStringOption.CreateStringOption(stringPrefab, container);
                __instance.AllItems = __instance.AllItems.AddItem(stringOption.transform).ToArray();
                continue;
            }

            if (!customStringOption.ShowInHideNSeek)
            {
                continue;
            }

            stringOption = customStringOption.CreateStringOption(stringPrefab, container);
            __instance.AllHideAndSeekItems = __instance.AllHideAndSeekItems.AddItem(stringOption.transform).ToArray();
        }
    }
}