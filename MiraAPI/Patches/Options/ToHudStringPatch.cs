using AmongUs.GameOptions;
using BepInEx;
using HarmonyLib;
using MiraAPI.API.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(IGameOptionsExtensions), "ToHudString")]
public static class ToHudStringPatch
{
    public static int CurrentPage = 0;
    /// <summary>
    /// Generic method to add options 
    /// </summary>
    public static void AddOptions(StringBuilder sb,
        IEnumerable<CustomNumberOption> numberOptions, IEnumerable<CustomStringOption> stringOptions, IEnumerable<CustomToggleOption> toggleOptions)
    {
        foreach (var numberOption in numberOptions.Where(x => !x.Hidden()))
        {
            if (GameManager.Instance.IsHideAndSeek() && !numberOption.ShowInHideNSeek)
            {
                continue;
            }

            sb.AppendLine(numberOption.Title + ": " + numberOption.Value + Helpers.GetSuffix(numberOption.SuffixType));
        }

        foreach (var toggleOption in toggleOptions.Where(x => !x.Hidden()))
        {
            if (GameManager.Instance.IsHideAndSeek() && !toggleOption.ShowInHideNSeek)
            {
                continue;
            }

            sb.AppendLine(toggleOption.Title + ": " + (toggleOption.Value ? "On" : "Off"));
        }

        foreach (var stringOption in stringOptions.Where(x => !x.Hidden()))
        {
            if (GameManager.Instance.IsHideAndSeek() && !stringOption.ShowInHideNSeek)
            {
                continue;
            }

            sb.AppendLine(stringOption.Title + ": " + stringOption.Value);
        }
    }

    public static void Prefix()
    {
        if (GameManager.Instance is null || GameManager.Instance.IsHideAndSeek())
        {
            return;
        }

        foreach (var role in CustomRoleManager.CustomRoles.Values)
        {
            var customRole = role as ICustomRole;
            if (customRole.IsGhostRole)
            {
                role.Role = RoleTypes.CrewmateGhost;
            }
        }
    }

    /// <summary>
    /// Update the HudOptions on the left of the screen if player is using Launchpad options
    /// </summary>
    public static void Postfix(IGameOptions __instance, ref string __result)
    {
        if (GameManager.Instance is null)
        {
            return;
        }

        foreach (var pair in CustomRoleManager.CustomRoles)
        {
            var customRole = pair.Value as ICustomRole;
            if (customRole.IsGhostRole)
            {
                pair.Value.Role = (RoleTypes)pair.Key;
            }
        }

        if (CurrentPage != 0)
        {
            PluginInfo currentPlugin = CustomOptionsManager.RegisteredMods[CurrentPage - 1];
            if (currentPlugin == null)
            {
                CurrentPage = 0;
                return;
            }

            var sb = new StringBuilder($"<size=180%><b>{currentPlugin.Metadata.Name} Options:</b></size>\n<size=130%>");
            var groupsWithRoles = CustomOptionsManager.CustomGroups.Where(group => group.AdvancedRole != null && group.ParentMod == currentPlugin);
            var groupsWithoutRoles = CustomOptionsManager.CustomGroups.Where(group => group.AdvancedRole == null && group.ParentMod == currentPlugin);

            AddOptions(sb,
                CustomOptionsManager.CustomNumberOptions.Where(option => option.Group == null && option.ParentMod == currentPlugin && !option.Hidden()),
                CustomOptionsManager.CustomStringOptions.Where(option => option.Group == null && option.ParentMod == currentPlugin && !option.Hidden()),
                CustomOptionsManager.CustomToggleOptions.Where(option => option.Group == null && option.ParentMod == currentPlugin && !option.Hidden())
                );

            foreach (var group in groupsWithoutRoles)
            {
                if (group.Hidden() || (GameManager.Instance.IsHideAndSeek() && !group.Options.Any(x => x.ShowInHideNSeek)))
                {
                    continue;
                }

                sb.AppendLine($"\n<size=160%><b>{group.Title}</b></size>");
                AddOptions(sb, group.CustomNumberOptions, group.CustomStringOptions, group.CustomToggleOptions);
            }

            var customOptionGroups = groupsWithRoles as CustomOptionGroup[] ?? groupsWithRoles.ToArray();
            if (customOptionGroups.Any())
            {
                sb.AppendLine("\n<size=160%><b>Roles</b></size>");

                foreach (var group in customOptionGroups)
                {
                    if (group.Hidden())
                    {
                        continue;
                    }

                    sb.AppendLine($"<size=140%><b>{group.Title}</b></size><size=120%>");
                    AddOptions(sb, group.CustomNumberOptions, group.CustomStringOptions, group.CustomToggleOptions);
                    sb.Append("</size>\n");
                }
            }

            __result = sb.ToString();
        }

        if (CurrentPage == 0)
        {
            __result = "<size=160%><b>Normal Options:</b></size>\n<size=130%>" + __result;
        }
        __result += $"\nUse <b>TAB</b> to switch pages (Page {CurrentPage}/{CustomOptionsManager.RegisteredMods.Count})</size>";
    }
}