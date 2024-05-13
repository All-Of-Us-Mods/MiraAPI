using HarmonyLib;
using MiraAPI.Roles;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(RoleOptionSetting))]
public static class RoleOptionSettingPatch
{
    /// <summary>
    /// Update the role details screen in role settings for Launchpad roles
    /// </summary>
    [HarmonyPrefix, HarmonyPatch("ShowRoleDetails")]
    public static bool ShowRoleDetailsPrefix(RoleOptionSetting __instance)
    {
        if (__instance.Role is ICustomRole customRole)
        {
            GameSettingMenu.Instance.RoleName.text = __instance.Role.NiceName;
            GameSettingMenu.Instance.RoleBlurb.text = __instance.Role.BlurbLong;
            GameSettingMenu.Instance.RoleIcon.sprite = customRole.Icon.LoadAsset();
            return false;
        }
        return true;
    }
}