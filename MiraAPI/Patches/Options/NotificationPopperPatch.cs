using HarmonyLib;
using Il2CppSystem;
using MiraAPI.Roles;
using System.Linq;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(NotificationPopper))]
public class NotificationPopperPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(NotificationPopper.AddRoleSettingsChangeMessage))]
    public static bool RoleChangeMsgPatch(NotificationPopper __instance,
        [HarmonyArgument(0)] StringNames key, [HarmonyArgument(1)] int roleCount, [HarmonyArgument(2)] int roleChance, [HarmonyArgument(3)] RoleTeamTypes teamType,
        [HarmonyArgument(4)] bool playSound)
    {
        if (CustomRoleManager.CustomRoles.Values.Any(role => role.StringName == key))
        {
            string item = string.Empty;
            string text = (teamType == RoleTeamTypes.Crewmate) ? Palette.CrewmateSettingChangeText.ToTextColor() : Palette.ImpostorRed.ToTextColor();
            item = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.LobbyChangeSettingNotificationRole, new Object[]
            {
            string.Concat(
            [
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">",
                text,
                DestroyableSingleton<TranslationController>.Instance.GetString(key, Array.Empty<Object>()),
                "</color></font>"
            ]),
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleCount.ToString() + "</font>",
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleChance.ToString() + "%"
            });
            __instance.SettingsChangeMessageLogic(key, item, playSound);
            return false;
        }

        return true;
    }
}