using HarmonyLib;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using System;
using System.Linq;
using MiraAPI.Networking;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(RolesSettingsMenu))]
public static class RoleSettingMenuPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RolesSettingsMenu.SetQuotaTab))]
    public static bool PatchStart(RolesSettingsMenu __instance)
    {
        __instance.roleChances = new Il2CppSystem.Collections.Generic.List<RoleOptionSetting>();
        __instance.advancedSettingChildren = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();
            
        var maskBg = __instance.scrollBar.transform.FindChild("MaskBg");

        if (GameSettingMenuPatches.CurrentSelectedMod == 0)
        {
            __instance.AllButton.transform.parent.gameObject.SetActive(true);
            __instance.AllButton.gameObject.SetActive(true);
            __instance.scrollBar.transform.localPosition = new Vector3(-1.4957f, 0.657f, -4);
            maskBg.localPosition = new Vector3(1.5353f, -.5734f, -.1f);
            maskBg.localScale = new Vector3(6.6811f, 3.3563f, 0.5598f);
            return true;
        }

        float num = 0.522f;

        __instance.AllButton.transform.parent.gameObject.SetActive(false);
        __instance.AllButton.gameObject.SetActive(false);
        __instance.scrollBar.transform.localPosition = new Vector3(-1.4957f, 1.5261f, -4);
        maskBg.localPosition = new Vector3(1.5353f, -1.0607f, -.1f);
        maskBg.localScale = new Vector3(6.6811f, 4.1563f, 0.5598f);

        CategoryHeaderEditRole categoryHeaderEditRole = Object.Instantiate(__instance.categoryHeaderEditRoleOrigin, Vector3.zero, Quaternion.identity, __instance.RoleChancesSettings.transform);
        categoryHeaderEditRole.SetHeader(StringNames.CrewmateRolesHeader, 20);
        categoryHeaderEditRole.transform.localPosition = new Vector3(4.986f, num, -2f);
        num -= 0.522f;
        int num3 = 0;

        foreach (var role in GameSettingMenuPatches.SelectedMod.CustomRoles.Values.OfType<ICustomRole>().Where(role => role.Team == ModdedRoleTeams.Crewmate))
        {
            CreateQuotaOption(__instance, role as RoleBehaviour, ref num, num3);
            num3++;
        }

        num -= 0.4f;
        CategoryHeaderEditRole categoryHeaderEditRole2 = Object.Instantiate(__instance.categoryHeaderEditRoleOrigin, Vector3.zero, Quaternion.identity, __instance.RoleChancesSettings.transform);
        categoryHeaderEditRole2.SetHeader(StringNames.ImpostorRolesHeader, 20);
        categoryHeaderEditRole2.transform.localPosition = new Vector3(4.986f, num, -2f);
        num -= 0.522f;


        foreach (var role in GameSettingMenuPatches.SelectedMod.CustomRoles.Values.OfType<ICustomRole>().Where(role => role.Team == ModdedRoleTeams.Impostor))
        {
            CreateQuotaOption(__instance, role as RoleBehaviour, ref num, num3);
            num3++;
        }

        num -= 0.8f;
        CategoryHeaderEditRole categoryHeaderEditRole3 = Object.Instantiate(__instance.categoryHeaderEditRoleOrigin, Vector3.zero, Quaternion.identity, __instance.RoleChancesSettings.transform);
        categoryHeaderEditRole3.SetHeader(StringNames.None, 20);
        categoryHeaderEditRole3.Title.text = "Neutral Roles";
        categoryHeaderEditRole3.transform.localPosition = new Vector3(4.986f, num, -2f);
        num -= 0.522f;


        foreach (var role in GameSettingMenuPatches.SelectedMod.CustomRoles.Values.OfType<ICustomRole>().Where(role => role.Team == ModdedRoleTeams.Neutral))
        {
            CreateQuotaOption(__instance, role as RoleBehaviour, ref num, num3);
            num3++;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RolesSettingsMenu.Update))]
    public static bool UpdatePatch()
    {
        if (GameSettingMenuPatches.CurrentSelectedMod == 0)
        {
            return true;
        }

        return false;
    }

    private static void ValueChanged(OptionBehaviour obj)
    {
        var roleSetting = obj.Cast<RoleOptionSetting>();
        var role = roleSetting.Role as ICustomRole;
        if (role is null or { HideSettings: true }) return;

        try
        {
            role.ParentMod.PluginConfig.TryGetEntry<int>(role.NumConfigDefinition, out var numEntry);
            numEntry.Value = roleSetting.RoleMaxCount;

            role.ParentMod.PluginConfig.TryGetEntry<int>(role.ChanceConfigDefinition, out var chanceEntry);
            chanceEntry.Value = roleSetting.RoleChance;
        }
        catch (Exception e)
        {
            Logger<MiraApiPlugin>.Warning(e);
        }

        roleSetting.UpdateValuesAndText(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);
        DestroyableSingleton<HudManager>.Instance.Notifier.AddRoleSettingsChangeMessage(roleSetting.Role.StringName, roleSetting.RoleMaxCount, roleSetting.RoleChance, roleSetting.Role.TeamType, false);

        if (AmongUsClient.Instance.AmHost)
        {
            Rpc<SyncRoleOptionsRpc>.Instance.Send(PlayerControl.LocalPlayer, [role.GetNetData(roleSetting.Role)], true);
        }
        GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
    }

    private static void CreateAdvancedSettings(RolesSettingsMenu __instance, RoleBehaviour role)
    {
        foreach (var option in __instance.advancedSettingChildren) 
        {
            Object.Destroy(option.gameObject);
        }

        __instance.advancedSettingChildren.Clear();

        float num = -0.872f;
        foreach (var option in GameSettingMenuPatches.SelectedMod.Options.Where(option => option.AdvancedRole != null && option.AdvancedRole == role.GetType()))
        {
            OptionBehaviour newOpt = option.CreateOption(__instance.checkboxOrigin, __instance.numberOptionOrigin, __instance.stringOptionOrigin, __instance.AdvancedRolesSettings.transform);
            newOpt.transform.localPosition = new Vector3(2.17f, num, -2f);
            newOpt.SetClickMask(__instance.ButtonClickMask);

            SpriteRenderer[] componentsInChildren = newOpt.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].material.SetInt(PlayerMaterial.MaskLayer, 20);
            }
            foreach (TextMeshPro textMeshPro in newOpt.GetComponentsInChildren<TextMeshPro>(true))
            {
                textMeshPro.fontMaterial.SetFloat(ShaderID.StencilComp, 3f);
                textMeshPro.fontMaterial.SetFloat(ShaderID.Stencil, 20);
            }

            newOpt.LabelBackground.enabled = false;
            __instance.advancedSettingChildren.Add(newOpt);

            num += -0.45f;
            newOpt.Initialize();
        }

        __instance.scrollBar.CalculateAndSetYBounds(__instance.advancedSettingChildren.Count + 3, 1f, 6f, 0.45f);
    }
    private static void ChangeTab(RoleBehaviour role, RolesSettingsMenu __instance)
    {
        ICustomRole customRole = role as ICustomRole;
        __instance.roleDescriptionText.text = customRole.RoleLongDescription;
        __instance.roleTitleText.text = DestroyableSingleton<TranslationController>.Instance.GetString(role.StringName, []);
        __instance.roleScreenshot.sprite = Sprite.Create(customRole.OptionsScreenshot.LoadAsset().texture, new Rect(0, 0, 370, 230), Vector2.one / 2, 100);
        __instance.roleScreenshot.drawMode = SpriteDrawMode.Sliced;
        __instance.roleHeaderSprite.color = customRole.RoleColor;
        __instance.roleHeaderText.color = customRole.RoleColor.DarkenColor();

        CreateAdvancedSettings(__instance, role);

        foreach (var optionBehaviour in __instance.advancedSettingChildren)
        {
            optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                optionBehaviour.SetAsPlayer();
            }
        }
        __instance.RoleChancesSettings.SetActive(false);
        __instance.AdvancedRolesSettings.SetActive(true);
        __instance.RefreshChildren();
    }

    public static void CreateQuotaOption(RolesSettingsMenu __instance, RoleBehaviour role, ref float yPos, int index)
    {
        ICustomRole customRole = role as ICustomRole;
        RoleOptionSetting roleOptionSetting = Object.Instantiate(__instance.roleOptionSettingOrigin, Vector3.zero, Quaternion.identity, __instance.RoleChancesSettings.transform);
        roleOptionSetting.transform.localPosition = new Vector3(-0.15f, yPos, -2f);
        roleOptionSetting.SetRole(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions, role, 20);
        roleOptionSetting.labelSprite.color = customRole.RoleColor;
        roleOptionSetting.titleText.color = customRole.RoleColor.DarkenColor();
        roleOptionSetting.OnValueChanged = new Action<OptionBehaviour>(ValueChanged);
        roleOptionSetting.titleText.horizontalAlignment = HorizontalAlignmentOptions.Left;
        roleOptionSetting.SetClickMask(__instance.ButtonClickMask);
        __instance.roleChances.Add(roleOptionSetting);

        PassiveButton newButton = Object.Instantiate(roleOptionSetting.buttons[0], roleOptionSetting.transform);
        newButton.name = "ConfigButton";
        newButton.transform.localPosition = new Vector3(0.2419f, -0.2582f, -2f);
        newButton.transform.FindChild("Plus_TMP").gameObject.DestroyImmediate();
        newButton.transform.FindChild("InactiveSprite").GetComponent<SpriteRenderer>().sprite = MiraAssets.Cog.LoadAsset();
        newButton.transform.FindChild("ActiveSprite").GetComponent<SpriteRenderer>().sprite = MiraAssets.CogActive.LoadAsset();

        PassiveButton passiveButton = newButton.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            ChangeTab(role, __instance);
        }));

        if (index < GameSettingMenuPatches.SelectedMod.CustomRoles.Count - 1)
        {
            yPos += -0.43f;
        }
    }
}