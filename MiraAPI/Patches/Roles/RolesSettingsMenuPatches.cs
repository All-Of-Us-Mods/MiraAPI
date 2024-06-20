using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime;
using MiraAPI.Roles;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using System.Linq;
using MiraAPI.Networking;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Roles;
/*
[HarmonyPatch(typeof(RolesSettingsMenu))]
public static class RolesSettingsMenuPatches
{
    /// <summary>
    /// Create role buttons and advanced role settings menu for every custom Launchpad role
    /// </summary>
    [HarmonyPrefix, HarmonyPatch("OnEnable")]
    public static void OnEnablePrefix(RolesSettingsMenu __instance)
    {
        var tabPrefab = __instance.AllAdvancedSettingTabs.ToArray()[1].Tab;
        foreach (var (key, role) in CustomRoleManager.CustomRoles)
        {
            if (__instance.AllAdvancedSettingTabs.ToArray().Any(x => (ushort)x.Type == key))
            {
                continue;
            }

            if (role is ICustomRole { HideSettings: true })
            {
                continue;
            }

            var numChanceOption = Object.Instantiate(__instance.SettingPrefab, __instance.ItemParent);
            numChanceOption.transform.localPosition = new Vector3(0.044f, 0, 0);
            numChanceOption.name = role.NiceName;
            numChanceOption.Role = role;
            __instance.AllRoleSettings.Add(numChanceOption);


            var newTab = Object.Instantiate(tabPrefab, __instance.AdvancedRolesSettings.transform);
            newTab.name = role.NiceName + " Settings";
            var toggleSet = Object.Instantiate(newTab.GetComponentInChildren<ToggleOption>(true));
            var numberSet = Object.Instantiate(newTab.GetComponentInChildren<NumberOption>(true));

            foreach (var option in newTab.GetComponentsInChildren<OptionBehaviour>())
            {
                option.gameObject.Destroy();
            }

            var startOffset = 0.5f;

            /*            foreach (var customOption in CustomOptionsManager.CustomOptions)
                        {
                            if (customOption.AdvancedRole is not null && customOption.AdvancedRole == role.GetType())
                            {
                                startOffset -= 0.5f;

                                switch (customOption)
                                {
                                    case CustomNumberOption numberOption:
                                        var numOpt = numberOption.CreateNumberOption(numberSet, newTab.transform);
                                        numOpt.transform.localPosition = new Vector3(-1.25f, startOffset, 0);
                                        break;

                                    case CustomToggleOption toggleOption:
                                        var togOpt = toggleOption.CreateToggleOption(toggleSet, newTab.transform);
                                        togOpt.transform.localPosition = new Vector3(-1.25f, startOffset, 0);
                                        break;
                                }
                            }
                        }*\/

            var tmp = newTab.GetComponentInChildren<TextTranslatorTMP>();
            tmp.defaultStr = role.NiceName;
            tmp.TargetText = role.StringName;

            var newAdvSet = new AdvancedRoleSettingsButton
            {
                Tab = newTab,
                Type = (RoleTypes)key
            };

            __instance.AllAdvancedSettingTabs.Add(newAdvSet);

            toggleSet.gameObject.Destroy();
            numberSet.gameObject.Destroy();
        }
    }

    /// <summary>
    /// Set custom role options OnValueChanged and re-enable the role settings menu scrolling
    /// </summary>
    [HarmonyPostfix, HarmonyPatch("Start")]
    public static void StartPostfix(RolesSettingsMenu __instance)
    {
        /*        foreach (var customOption in CustomOptionsManager.CustomOptions)
                {
                    if (customOption.AdvancedRole is not null)
                    {
                        customOption.OptionBehaviour.OnValueChanged = (Action<OptionBehaviour>)customOption.ValueChanged;
                    }
                }*\/

        var scroll = __instance.GetComponentInChildren<Scroller>();
        scroll.active = true;
        scroll.ContentYBounds.max = 2 * __instance.AllRoleSettings.Count / 10f;

        scroll.transform.FindChild("UI_Scrollbar").gameObject.SetActive(true);
    }

    /// <summary>
    /// Update config when value changed
    /// </summary>
    [HarmonyPrefix, HarmonyPatch("ValueChanged")]
    public static bool ValueChangedPrefix(RolesSettingsMenu __instance, [HarmonyArgument(0)] OptionBehaviour obj)
    {
        if (obj.GetIl2CppType() != Il2CppType.Of<RoleOptionSetting>())
        {
            return true;
        }

        var roleSetting = obj.Cast<RoleOptionSetting>();

        if (roleSetting.Role is not ICustomRole role || role.HideSettings)
        {
            return true;
        }

        MiraAPIPlugin.Instance.Config.TryGetEntry<int>(role.NumConfigDefinition, out var numEntry);
        numEntry.Value = roleSetting.RoleMaxCount;

        MiraAPIPlugin.Instance.Config.TryGetEntry<int>(role.ChanceConfigDefinition, out var chanceEntry);
        chanceEntry.Value = roleSetting.RoleChance;

        roleSetting.UpdateValuesAndText(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);

        if (AmongUsClient.Instance.AmHost)
        {
            Rpc<SyncRoleOptionsRpc>.Instance.Send(new SyncRoleOptionsRpc.Data((ushort)roleSetting.Role.Role, numEntry.Value, chanceEntry.Value));
        }
        GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
        return false;
    }
}
*/