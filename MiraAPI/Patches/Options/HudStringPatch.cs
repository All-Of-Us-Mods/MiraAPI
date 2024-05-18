using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
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
        List<IModdedOption> options)
    {
        foreach (var opt in options)
        {
            sb.AppendLine(opt.GetHudStringText());
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
            IMiraConfig currentPlugin = ModdedOptionsManager.RegisteredMods.Values.ToArray()[CurrentPage - 1];
            if (currentPlugin == null)
            {
                CurrentPage = 0;
                return;
            }

            var sb = new StringBuilder($"<size=180%><b>{currentPlugin.TabSettings.Title} Options:</b></size>\n<size=130%>");
            var groupsWithRoles = ModdedOptionsManager.Groups.Where(group => group.AdvancedRole != null && group.ParentMod == currentPlugin);
            var groupsWithoutRoles = ModdedOptionsManager.Groups.Where(group => group.AdvancedRole == null && group.ParentMod == currentPlugin);

            AddOptions(sb,
                ModdedOptionsManager.Options.Where(option => option.Group == null && option.ParentMod == currentPlugin && option.Visible()).ToList()
                );

            foreach (var group in groupsWithoutRoles)
            {
                if (!group.GroupVisible())
                {
                    continue;
                }

                sb.AppendLine($"\n<size=160%><b>{group.GroupColor.ToTextColor()}{group.GroupName}</color></b></size>");
                AddOptions(sb, ModdedOptionsManager.Options.Where(x => x.Group == group).ToList());
            }

            var customOptionGroups = groupsWithRoles as ModdedOptionGroup[] ?? groupsWithRoles.ToArray();
            if (customOptionGroups.Any())
            {
                sb.AppendLine("\n<size=160%><b>Roles</b></size>");

                foreach (var group in customOptionGroups)
                {
                    if (!group.GroupVisible())
                    {
                        continue;
                    }

                    sb.AppendLine($"<size=140%><b>{group.GroupColor.ToTextColor()}{group.GroupName}</color></b></size><size=120%>");
                    AddOptions(sb, ModdedOptionsManager.Options.Where(x => x.Group == group).ToList());
                    sb.Append("</size>\n");
                }
            }

            __result = sb.ToString();
        }

        if (CurrentPage == 0)
        {
            __result = "<size=160%><b>Normal Options:</b></size>\n<size=130%>" + __result;
        }
        __result += $"\nUse <b>TAB</b> to switch pages (Page {CurrentPage}/{ModdedOptionsManager.RegisteredMods.Count})</size>";
    }
}