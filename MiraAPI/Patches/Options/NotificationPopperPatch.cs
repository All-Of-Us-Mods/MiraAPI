using System.Linq;
using HarmonyLib;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(NotificationPopper))]
public static class NotificationPopperPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(NotificationPopper.AddRoleSettingsChangeMessage))]
    public static bool RoleChangeMsgPatch(
        NotificationPopper __instance,
        [HarmonyArgument(0)] StringNames key,
        [HarmonyArgument(1)] int roleCount,
        [HarmonyArgument(2)] int roleChance,
        [HarmonyArgument(4)] bool playSound)
    {
        if (CustomRoleManager.CustomRoles.Values.FirstOrDefault(x => x.StringName == key) is not ICustomRole customRole)
        {
            return true;
        }

        var textColor = customRole.OptionsMenuColor.ToTextColor();

        var icon = customRole.Configuration.IconTmp ? $"<sprite name=\"{customRole.Configuration.IconTmp.name}\">" : string.Empty;
        var item = TranslationController.Instance.GetString(
            StringNames.LobbyChangeSettingNotificationRole,
            string.Concat(
                icon,
                "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">",
                textColor,
                TranslationController.Instance.GetString(key),
                "</color></font>"
            ),
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleCount + "</font>",
            "<font=\"Barlow-Black SDF\" material=\"Barlow-Black Outline\">" + roleChance + "%"
        );

        __instance.SettingsChangeMessageLogic(key, item, playSound);
        return false;
    }
}
