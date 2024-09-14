using HarmonyLib;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(LobbyViewSettingsPane))]
public static class LobbyViewPanePatches
{
    private static int CurrentSelectedMod { get; set; }

    private static MiraPluginInfo? SelectedMod => CurrentSelectedMod == 0
        ? null
        : MiraPluginManager.Instance.RegisteredPlugins()[CurrentSelectedMod - 1];

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
    public static void AwakePatch(LobbyViewSettingsPane __instance)
    {
        __instance.gameModeText.transform.localPosition = new Vector3(-2.3f, 2.4f, -2);

        // Create the next button
        var nextButton = Object.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        nextButton.GetComponent<BoxCollider2D>().size = new Vector2(0.2f, 0.3f);
        nextButton.transform.localPosition = new Vector3(-5.4f, 2.4f, -2f);
        nextButton.transform.localScale = new Vector3(3, 3, 2);
        nextButton.name = "RightArrowButton";

        var normal = nextButton.transform.FindChild("Normal").GetComponentInChildren<SpriteRenderer>();
        normal.transform.localPosition = new Vector3(0, 0f, 0.3f);
        normal.sprite = MiraAssets.NextButton.LoadAsset();

        var hover = nextButton.transform.FindChild("Hover").GetComponentInChildren<SpriteRenderer>();
        hover.transform.localPosition = new Vector3(0, 0f, 0.3f);
        hover.sprite = MiraAssets.NextButtonActive.LoadAsset();

        var passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                CurrentSelectedMod += 1;
                if (CurrentSelectedMod > MiraPluginManager.Instance.RegisteredPlugins().Length)
                {
                    CurrentSelectedMod = 0;
                }

                __instance.RefreshTab();
                __instance.scrollBar.ScrollToTop();
            }));

        // Create the back button
        var backButton = Object.Instantiate(nextButton, __instance.BackButton.transform.parent).gameObject;
        backButton.transform.localPosition = new Vector3(-6.3f, 2.4f, -2f);
        backButton.name = "LeftArrowButton";
        backButton.transform.FindChild("Normal").gameObject.GetComponentInChildren<SpriteRenderer>().flipX
            = backButton.transform.FindChild("Hover").gameObject.GetComponentInChildren<SpriteRenderer>().flipX
                = true;

        var passiveButton2 = backButton.gameObject.GetComponent<PassiveButton>();
        passiveButton2.OnClick = new Button.ButtonClickedEvent();
        passiveButton2.OnClick.AddListener(
            (UnityAction)(() =>
            {
                CurrentSelectedMod -= 1;
                if (CurrentSelectedMod < 0)
                {
                    CurrentSelectedMod = MiraPluginManager.Instance.RegisteredPlugins().Length;
                }

                __instance.RefreshTab();
                __instance.scrollBar.ScrollToTop();
            }));
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.Update))]
    public static void UpdatePatch(LobbyViewSettingsPane __instance)
    {
        __instance.gameModeText.text = SelectedMod?.PluginInfo.Metadata.Name ?? "Default";
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawNormalTab))]
    public static bool DrawNormalTabPatch(LobbyViewSettingsPane __instance)
    {
        if (CurrentSelectedMod == 0)
        {
            return true;
        }

        DrawOptionsTab(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawRolesTab))]
    public static bool DrawRolesTabPatch(LobbyViewSettingsPane __instance)
    {
        if (CurrentSelectedMod == 0)
        {
            return true;
        }

        DrawRolesTab(__instance);
        return false;
    }

    private static void DrawOptionsTab(LobbyViewSettingsPane instance)
    {
        if (SelectedMod == null)
        {
            return;
        }

        var num = 1.44f;

        var filteredGroups = SelectedMod.OptionGroups.Where(x => x.GroupVisible.Invoke() && x.AdvancedRole is null);

        foreach (var group in filteredGroups)
        {
            var categoryHeaderMasked = Object.Instantiate(
                instance.categoryHeaderOrigin,
                instance.settingsContainer,
                true);
            categoryHeaderMasked.SetHeader(StringNames.Name, 61);
            categoryHeaderMasked.Title.text = group.GroupName;
            categoryHeaderMasked.transform.localScale = Vector3.one;
            categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
            instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
            num -= 0.85f;

            var i = 0;

            foreach (var option in group.Options)
            {
                if (!option.Visible.Invoke())
                {
                    continue;
                }

                var viewSettingsInfoPanel = Object.Instantiate(
                    instance.infoPanelOrigin,
                    instance.settingsContainer,
                    true);
                viewSettingsInfoPanel.transform.localScale = Vector3.one;
                float num2;
                if (i % 2 == 0)
                {
                    num2 = -8.95f;
                    if (i > 0)
                    {
                        num -= 0.59f;
                    }
                }
                else
                {
                    num2 = -3f;
                }

                viewSettingsInfoPanel.transform.localPosition = new Vector3(num2, num, -2f);

                var data = option.Data;

                if (data.Type == OptionTypes.Checkbox)
                {
                    viewSettingsInfoPanel.SetInfoCheckbox(
                        data.Title,
                        61,
                        Mathf.Approximately(option.GetFloatData(), 1));
                }
                else
                {
                    viewSettingsInfoPanel.SetInfo(data.Title, data.GetValueString(option.GetFloatData()), 61);
                }

                instance.settingsInfo.Add(viewSettingsInfoPanel.gameObject);
                i++;
            }

            num -= 0.59f;
        }

        instance.scrollBar.CalculateAndSetYBounds(instance.settingsInfo.Count + 10, 2f, 6f, 0.59f);
    }

    private static void DrawRolesTab(LobbyViewSettingsPane instance)
    {
        if (SelectedMod == null)
        {
            return;
        }

        var num = 0.95f;
        var num2 = -6.53f;
        var categoryHeaderMasked =
            Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
        categoryHeaderMasked.SetHeader(StringNames.RoleQuotaLabel, 61);
        categoryHeaderMasked.transform.localScale = Vector3.one;
        categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, 1.26f, -2f);
        instance.settingsInfo.Add(categoryHeaderMasked.gameObject);

        var list = new List<Type>();

        foreach (var team in Enum.GetValues<ModdedRoleTeams>())
        {
            var filteredRoles = SelectedMod.CustomRoles.Values
                .OfType<ICustomRole>()
                .Where(x => !x.Configuration.HideSettings && x.Team == team).ToList();

            if (filteredRoles.Count == 0)
            {
                continue;
            }

            var categoryHeaderRoleVariant =
                Object.Instantiate(instance.categoryHeaderRoleOrigin, instance.settingsContainer, true);

            switch (team)
            {
                case ModdedRoleTeams.Crewmate:
                    categoryHeaderRoleVariant.SetHeader(StringNames.CrewmateRolesHeader, 61);
                    break;
                case ModdedRoleTeams.Impostor:
                    categoryHeaderRoleVariant.SetHeader(StringNames.ImpostorRolesHeader, 61);
                    break;
                default:
                    categoryHeaderRoleVariant.SetHeader(StringNames.CrewmateRolesHeader, 61);
                    categoryHeaderRoleVariant.Title.text = team + " Roles";
                    categoryHeaderRoleVariant.Background.color = Color.gray;
                    categoryHeaderRoleVariant.Title.color = Color.white;
                    break;
            }

            categoryHeaderRoleVariant.transform.localScale = Vector3.one;
            categoryHeaderRoleVariant.transform.localPosition = new Vector3(0.09f, num, -2f);
            instance.settingsInfo.Add(categoryHeaderRoleVariant.gameObject);
            num -= 0.696f;

            foreach (var customRole in filteredRoles)
            {
                var roleBehaviour = customRole as RoleBehaviour;
                if (roleBehaviour == null)
                {
                    continue;
                }

                var chancePerGame =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleBehaviour.Role);
                var numPerGame =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleBehaviour.Role);

                var flag = numPerGame == 0;

                var viewSettingsInfoPanelRoleVariant =
                    Object.Instantiate(
                        instance.infoPanelRoleOrigin,
                        instance.settingsContainer,
                        true);
                viewSettingsInfoPanelRoleVariant.transform.localScale = Vector3.one;
                viewSettingsInfoPanelRoleVariant.transform.localPosition = new Vector3(num2, num, -2f);

                var advancedRoleOptions = SelectedMod.Options
                    .Where(x => x.AdvancedRole == customRole.GetType())
                    .ToList();

                if (!flag && advancedRoleOptions.Count > 0)
                {
                    list.Add(customRole.GetType());
                }

                viewSettingsInfoPanelRoleVariant.SetInfo(
                    roleBehaviour.NiceName,
                    numPerGame,
                    chancePerGame,
                    61,
                    customRole.RoleColor,
                    customRole.Configuration.Icon.LoadAsset(),
                    true);

                viewSettingsInfoPanelRoleVariant.titleText.color =
                    viewSettingsInfoPanelRoleVariant.chanceTitle.color =
                        viewSettingsInfoPanelRoleVariant.chanceBackground.color =
                            viewSettingsInfoPanelRoleVariant.background.color =
                                customRole.RoleColor.GetAlternateColor();
                instance.settingsInfo.Add(viewSettingsInfoPanelRoleVariant.gameObject);
                num -= 0.664f;
            }
        }

        if (list.Count > 0)
        {
            var categoryHeaderMasked2 =
                Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
            categoryHeaderMasked2.SetHeader(StringNames.RoleSettingsLabel, 61);
            categoryHeaderMasked2.transform.localScale = Vector3.one;
            categoryHeaderMasked2.transform.localPosition = new Vector3(-9.77f, num, -2f);
            instance.settingsInfo.Add(categoryHeaderMasked2.gameObject);
            num -= 1.7f;
            var num3 = 0f;
            for (var k = 0; k < list.Count; k++)
            {
                float num4;
                if (k % 2 == 0)
                {
                    num4 = -5.8f;
                    if (k > 0)
                    {
                        num -= num3 + 0.59f;
                        num3 = 0f;
                    }
                }
                else
                {
                    num4 = 0.14999962f;
                }

                var advancedRoleViewPanel =
                    Object.Instantiate(instance.advancedRolePanelOrigin, instance.settingsContainer, true);
                advancedRoleViewPanel.transform.localScale = Vector3.one;
                advancedRoleViewPanel.transform.localPosition = new Vector3(num4, num, -2f);
                var num5 = SetUpAdvancedRoleViewPanel(advancedRoleViewPanel, list[k], 0.59f, 61);

                if (num5 > num3)
                {
                    num3 = num5;
                }

                instance.settingsInfo.Add(advancedRoleViewPanel.gameObject);
            }
        }

        instance.scrollBar.SetYBoundsMax(-num);
    }

    private static float SetUpAdvancedRoleViewPanel(
        AdvancedRoleViewPanel viewPanel,
        Type roleType,
        float spacingY,
        int maskLayer)
    {
        if (SelectedMod == null)
        {
            return 0;
        }

        var role = SelectedMod.CustomRoles.Values.FirstOrDefault(x => x.GetType() == roleType);

        if (role == null)
        {
            return 0;
        }

        if (role is not ICustomRole customRole)
        {
            return 0;
        }

        viewPanel.header.SetHeader(
            role.StringName,
            maskLayer,
            role.TeamType == RoleTeamTypes.Crewmate,
            customRole.Configuration.Icon.LoadAsset());
        viewPanel.divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);

        var num = viewPanel.yPosStart;
        var num2 = 1.08f;

        var filteredOptions = SelectedMod.Options
            .Where(x => x.AdvancedRole == roleType)
            .ToList();

        for (var i = 0; i < filteredOptions.Count; i++)
        {
            var option = filteredOptions[i];
            var baseGameSetting = option.Data;
            var viewSettingsInfoPanel = Object.Instantiate(viewPanel.infoPanelOrigin, viewPanel.transform, true);
            viewSettingsInfoPanel.transform.localScale = Vector3.one;
            viewSettingsInfoPanel.transform.localPosition = new Vector3(viewPanel.xPosStart, num, -2f);

            var value = option.GetFloatData();

            if (baseGameSetting.Type == OptionTypes.Checkbox)
            {
                viewSettingsInfoPanel.SetInfoCheckbox(baseGameSetting.Title, maskLayer, value > 0f);
            }
            else
            {
                viewSettingsInfoPanel.SetInfo(baseGameSetting.Title, baseGameSetting.GetValueString(value), maskLayer);
            }

            num -= spacingY;
            if (i > 0)
            {
                num2 += 0.8f;
            }
        }

        return num2;
    }
}
