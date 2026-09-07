using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(LobbyViewSettingsPane))]
public static class LobbyViewPanePatches
{
    private static int SelectedModIdx { get; set; }

    private static MiraPluginInfo[] Plugins => MiraPluginManager.Instance.RegisteredPluginsWithOptions;

    private static MiraPluginInfo? SelectedMod => SelectedModIdx == 0
        ? null
        : Plugins[SelectedModIdx - 1];

    private static PassiveButton? ModifiersTabButton { get; set; }
    internal static StringNames ModifiersTabName { get; } = MiraLocaleManager.GetOrCreateLocaleString("ModifiersTab");

    private static AbstractGameMode? GetCustomGamemode()
    {
        return !CustomGameModeManager.IsClassic() && !CustomGameModeManager.IsHideNSeek() ? CustomGameModeManager.ActiveMode : null;
    }

    private static IEnumerable<AbstractOptionGroup>? GetCustomGamemodeOptions()
    {
        if (CustomGameModeManager.ActiveMode == null)
        {
            return null;
        }

        if (!CustomGameModeManager.IsClassic() && !CustomGameModeManager.IsHideNSeek())
        {
            var plugin = CustomGameModeManager.FindParentMod(CustomGameModeManager.ActiveMode);

            return plugin?.InternalOptionGroups
                .Where(x => x.OptionableType?.IsAssignableTo(typeof(AbstractGameMode)) == true && x.GroupVisible());
        }

        return null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LobbyViewSettingsPane), nameof(LobbyViewSettingsPane.OnEnable))]
    public static void OnEnable(LobbyViewSettingsPane __instance)
    {
        Refresh(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.Awake))]
    public static void AwakePatch(LobbyViewSettingsPane __instance)
    {
        __instance.gameModeText.transform.localPosition = new Vector3(-2.3f, 2.4f, -2);
        __instance.gameModeText.GetComponent<TextTranslatorTMP>().Destroy();

        // create modifiers button
        ModifiersTabButton = Object.Instantiate(__instance.rolesTabButton, __instance.rolesTabButton.transform.parent);
        ModifiersTabButton.buttonText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        ModifiersTabButton.name = "ModifiersTabButton";
        var pos = ModifiersTabButton.transform.localPosition;
        pos.x = 2.1f;
        ModifiersTabButton.transform.localPosition = pos;
        ModifiersTabButton.buttonText.text = "Modifiers".Translate();
        ModifiersTabButton.OnClick = new Button.ButtonClickedEvent();
        ModifiersTabButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(ModifiersTabName);
            }));
        ModifiersTabButton.gameObject.SetActive(SelectedModIdx != 0);

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
                SelectedModIdx += 1;
                if (SelectedModIdx > Plugins.Length)
                {
                    SelectedModIdx = 0;
                }

                Refresh(__instance);
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
                SelectedModIdx -= 1;
                if (SelectedModIdx < 0)
                {
                    SelectedModIdx = Plugins.Length;
                }

                Refresh(__instance);
            }));
    }

    private static void Refresh(LobbyViewSettingsPane menu)
    {
        if (SelectedMod == null)
        {
            var cgm = GetCustomGamemode();
            ModifiersTabButton?.gameObject.SetActive(false);
            menu.rolesTabButton.gameObject.SetActive(cgm is null or { ShowNormalRoleSettings: true });
            menu.gameModeText.text = cgm is null ? MiraLocaleManager.Get("MiraApi.Gamemode.Classic") : cgm.ColoredName;
        }
        else
        {
            var showModifierButton = SelectedMod.InternalOptionGroups.Any(m =>
                m.ParentMenu is MenuCategory.Modifiers || m.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true);
            var showRolesButton = SelectedMod.InternalOptionGroups.Any(r =>
                r.ParentMenu is MenuCategory.Roles || r.OptionableType?.IsAssignableTo(typeof(ICustomRole)) == true);

            ModifiersTabButton?.gameObject.SetActive(showModifierButton);
            menu.rolesTabButton.gameObject.SetActive(showRolesButton);
            menu.gameModeText.text = SelectedMod.PluginInfo.Metadata.Name;
        }

        menu.RefreshTab();
        menu.scrollBar.ScrollToTop();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.ChangeTab))]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.RefreshTab))]
    // CHANGED BECAUSE OF INLINING
    public static void SetTabPatch(LobbyViewSettingsPane __instance)
    {
        if (__instance.currentTab != ModifiersTabName || SelectedMod == null)
        {
            ModifiersTabButton?.SelectButton(false);
            return;
        }

        __instance.taskTabButton.SelectButton(false);
        __instance.rolesTabButton.SelectButton(false);

        ModifiersTabButton?.SelectButton(true);

        var filteredGroups = SelectedMod.InternalOptionGroups
            .Where(x => x.GroupVisible() && (x.ParentMenu is MenuCategory.Modifiers || (x.OptionableType != null && x.OptionableType.IsAssignableTo(typeof(BaseModifier)))));

        DrawOptions(__instance, filteredGroups);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawNormalTab))]
    public static bool DrawNormalTabPatch(LobbyViewSettingsPane __instance)
    {
        if (SelectedMod == null)
        {
            var gamemodeOpts = GetCustomGamemodeOptions();
            if (gamemodeOpts != null)
            {
                DrawOptions(__instance, gamemodeOpts);
                return false;
            }

            return true;
        }

        if (__instance.currentTab == ModifiersTabName)
        {
            return false;
        }

        var filteredGroups = SelectedMod.InternalOptionGroups
            .Where(x => x is { ParentMenu: not MenuCategory.Modifiers, OptionableType: null } && x.GroupVisible());

        DrawOptions(__instance, filteredGroups);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(LobbyViewSettingsPane.DrawRolesTab))]
    public static bool DrawRolesTabPatch(LobbyViewSettingsPane __instance)
    {
        if (SelectedModIdx == 0)
        {
            var num = 0.95f;
            var num2 = -6.53f;
            var categoryHeaderMasked =
                Object.Instantiate(__instance.categoryHeaderOrigin, __instance.settingsContainer);
            categoryHeaderMasked.SetHeader(StringNames.RoleQuotaLabel, 61);
            categoryHeaderMasked.transform.localScale = Vector3.one;
            categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, 1.26f, -2f);
            __instance.settingsInfo.Add(categoryHeaderMasked.gameObject);
            var list = new List<RoleBehaviour>();
            for (var i = 0; i < 2; i++)
            {
                var categoryHeaderRoleVariant =
                    Object.Instantiate(__instance.categoryHeaderRoleOrigin, __instance.settingsContainer);
                categoryHeaderRoleVariant.SetHeader(
                    (i == 0) ? StringNames.CrewmateRolesHeader : StringNames.ImpostorRolesHeader,
                    61);
                categoryHeaderRoleVariant.transform.localScale = Vector3.one;
                categoryHeaderRoleVariant.transform.localPosition = new Vector3(0.09f, num, -2f);
                __instance.settingsInfo.Add(categoryHeaderRoleVariant.gameObject);
                num -= 0.696f;
                var roles = RoleManager.Instance.AllRoles.ToArray().Where(x =>
                    x is not ICustomRole && !x.IsRoleBlacklisted() && x.Role != RoleTypes.Crewmate &&
                    x.Role != RoleTypes.Impostor && ((i == 0 && x.TeamType is RoleTeamTypes.Crewmate) ||
                                                     (i == 1 && x.TeamType is RoleTeamTypes.Impostor)) &&
                    x.Role != RoleTypes.CrewmateGhost && x.Role != RoleTypes.ImpostorGhost).ToList();
                for (var j = 0; j < roles.Count; j++)
                {
                    var roleBehaviour = roles[j];
                    var chancePerGame =
                        GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(
                            roleBehaviour.Role);
                    var numPerGame =
                        GameOptionsManager.Instance.CurrentGameOptions.RoleOptions
                            .GetNumPerGame(roleBehaviour.Role);
                    var flag = numPerGame == 0;
                    var viewSettingsInfoPanelRoleVariant =
                        Object.Instantiate(
                            __instance.infoPanelRoleOrigin,
                            __instance.settingsContainer);
                    viewSettingsInfoPanelRoleVariant.transform.localScale = Vector3.one;
                    viewSettingsInfoPanelRoleVariant.transform.localPosition = new Vector3(num2, num, -2f);
                    if (!flag)
                    {
                        list.Add(roleBehaviour);
                    }

                    var color = (i == 0) ? Palette.CrewmateRoleBlue : Palette.ImpostorRoleRed;
                    if (roleBehaviour is ICustomRole { Team: not (ModdedRoleTeams.Crewmate or ModdedRoleTeams.Impostor) })
                    {
                        color = Color.grey;
                    }

                    viewSettingsInfoPanelRoleVariant.SetInfo(
                        roleBehaviour.NiceName,
                        numPerGame,
                        chancePerGame,
                        61,
                        color,
                        roleBehaviour.RoleIconSolid,
                        i == 0,
                        flag);
                    __instance.settingsInfo.Add(viewSettingsInfoPanelRoleVariant.gameObject);
                    num -= 0.664f;
                }
            }

            if (list.Count > 0)
            {
                var categoryHeaderMasked2 =
                    Object.Instantiate(__instance.categoryHeaderOrigin, __instance.settingsContainer);
                categoryHeaderMasked2.SetHeader(StringNames.RoleSettingsLabel, 61);
                categoryHeaderMasked2.transform.localScale = Vector3.one;
                categoryHeaderMasked2.transform.localPosition = new Vector3(-9.77f, num, -2f);
                __instance.settingsInfo.Add(categoryHeaderMasked2.gameObject);
                num -= 2.1f;
                var num3 = 0f;
                for (var k = 0; k < list.Count; k++)
                {
                    float num4;
                    if (k % 2 == 0)
                    {
                        num4 = -5.8f;
                        if (k > 0)
                        {
                            num -= num3 + 0.85f;
                            num3 = 0f;
                        }
                    }
                    else
                    {
                        num4 = 0.14999962f;
                    }

                    var advancedRoleViewPanel =
                        Object.Instantiate(__instance.advancedRolePanelOrigin, __instance.settingsContainer);
                    advancedRoleViewPanel.transform.localScale = Vector3.one;
                    advancedRoleViewPanel.transform.localPosition = new Vector3(num4, num, -2f);
                    var num5 = advancedRoleViewPanel.SetUp(list[k], 0.85f, 61);
                    if (num5 > num3)
                    {
                        num3 = num5;
                    }

                    __instance.settingsInfo.Add(advancedRoleViewPanel.gameObject);
                }
            }

            __instance.scrollBar.SetYBoundsMax(-num);

            return false;
        }

        if (__instance.currentTab == ModifiersTabName)
        {
            return false;
        }

        DrawRolesTab(__instance);
        return false;
    }

    private static void DrawOptions(LobbyViewSettingsPane menu, IEnumerable<AbstractOptionGroup> groups)
    {
        var num = 1.44f;

        var groupArray = groups.Where(x => x.GroupVisible() && x.Options.Any(y => y.Visible())).ToArray();

        foreach (var group in groupArray)
        {
            var categoryHeaderMasked = Object.Instantiate(
                menu.categoryHeaderOrigin,
                menu.settingsContainer,
                true);

            categoryHeaderMasked.SetHeader(StringNames.Name, 61);
            categoryHeaderMasked.Title.text = group.GroupName.Translate();
            categoryHeaderMasked.transform.localScale = Vector3.one;
            categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, num, -2f);
            menu.settingsInfo.Add(categoryHeaderMasked.gameObject);
            num -= 1.05f;

            var i = 0;

            foreach (var option in group.Options)
            {
                if (!option.Visible())
                {
                    continue;
                }

                var viewSettingsInfoPanel = Object.Instantiate(
                    menu.infoPanelOrigin,
                    menu.settingsContainer,
                    true);

                viewSettingsInfoPanel.transform.localScale = Vector3.one;
                float num2;
                if (i % 2 == 0)
                {
                    num2 = -8.95f;
                    if (i > 0)
                    {
                        num -= 0.85f;
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

                    if (viewSettingsInfoPanel.titleText.text.Contains("Game Mode"))
                    {
                        viewSettingsInfoPanel.titleText.text = TranslationController.Instance.GetString(
                            data.Title, CustomGameModeManager.ActiveMode != null ? $"<color=#{CustomGameModeManager.ActiveMode.Color.ToHtmlStringRGBA()}>{CustomGameModeManager.ActiveMode.Name}</color>" : "Classic"
                        );
                    }
                }

                menu.settingsInfo.Add(viewSettingsInfoPanel.gameObject);
                i++;
            }

            num -= 0.85f;
        }

        menu.scrollBar.SetYBoundsMax(-num - 2);
    }

    private static void DrawRolesTab(LobbyViewSettingsPane instance)
    {
        if (SelectedMod == null)
        {
            return;
        }

        var num = 0.95f;
        const float num2 = -6.53f;
        var categoryHeaderMasked =
            Object.Instantiate(instance.categoryHeaderOrigin, instance.settingsContainer, true);
        categoryHeaderMasked.SetHeader(StringNames.RoleQuotaLabel, 61);
        categoryHeaderMasked.transform.localScale = Vector3.one;
        categoryHeaderMasked.transform.localPosition = new Vector3(-9.77f, 1.26f, -2f);
        instance.settingsInfo.Add(categoryHeaderMasked.gameObject);

        var list = new List<Type>();

        var roleGroups = SelectedMod.InternalRoles.Values.OfType<ICustomRole>()
            .ToLookup(x => x.RoleOptionsGroup);

        // sort the groups by priority
        var sortedRoleGroups = roleGroups
            .OrderBy(x => x.Key.Priority)
            .ThenBy(x => x.Key.Name.Translate());

        foreach (var grouping in sortedRoleGroups)
        {
            if (!grouping.Any() || grouping.All(x => x.Configuration.HideSettings || !x.VisibleInSettings() || !x.Configuration.AssociatedGameMode.IsInstanceOfType(CustomGameModeManager.ActiveMode)))
            {
                continue;
            }

            var group = grouping.Key;

            var name = MiraLocaleManager.GetOrCreateLocaleString(group.Name);

            var categoryHeaderRoleVariant = Object.Instantiate(instance.categoryHeaderRoleOrigin, instance.settingsContainer, true);
            categoryHeaderRoleVariant.SetHeader(name, 61);

            if (name is not (StringNames.CrewmateRolesHeader or StringNames.ImpostorRolesHeader))
            {
                var veryDarkColor = group.Color.DarkenColor(.35f);
                categoryHeaderRoleVariant.Title.color = veryDarkColor;
                categoryHeaderRoleVariant.Background.color = group.Color;
            }

            categoryHeaderRoleVariant.transform.localScale = Vector3.one;
            categoryHeaderRoleVariant.transform.localPosition = new Vector3(0.09f, num, -2f);
            instance.settingsInfo.Add(categoryHeaderRoleVariant.gameObject);
            num -= 0.696f;

            foreach (var customRole in grouping)
            {
                if (customRole.Configuration.HideSettings || !customRole.VisibleInSettings() || !customRole.Configuration.AssociatedGameMode.IsInstanceOfType(CustomGameModeManager.ActiveMode))
                {
                    continue;
                }

                var roleBehaviour = customRole as RoleBehaviour;
                if (roleBehaviour == null)
                {
                    continue;
                }

                var chancePerGame =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetChancePerGame(roleBehaviour.Role);
                var numPerGame =
                    GameOptionsManager.Instance.CurrentGameOptions.RoleOptions.GetNumPerGame(roleBehaviour.Role);

                var viewSettingsInfoPanelRoleVariant =
                    Object.Instantiate(
                        instance.infoPanelRoleOrigin,
                        instance.settingsContainer,
                        true);
                viewSettingsInfoPanelRoleVariant.transform.localScale = Vector3.one;
                viewSettingsInfoPanelRoleVariant.transform.localPosition = new Vector3(num2, num, -2f);

                var advancedRoleOptions = SelectedMod.InternalOptionGroups
                    .Where(x => x.OptionableType == customRole.GetType())
                    .SelectMany(x => x.Options)
                    .ToList();

                if (numPerGame > 0 && advancedRoleOptions.Count > 0)
                {
                    list.Add(customRole.GetType());
                }

                viewSettingsInfoPanelRoleVariant.SetInfo(
                    roleBehaviour.GetRoleName(),
                    numPerGame,
                    chancePerGame,
                    61,
                    customRole.RoleColor,
                    customRole.Configuration.Icon?.LoadAsset() ?? MiraAssets.Empty.LoadAsset(),
                    true);
                viewSettingsInfoPanelRoleVariant.iconSprite.transform.localScale = new Vector3(0.365f, 0.365f, 1f);
                viewSettingsInfoPanelRoleVariant.iconSprite.transform.localPosition = new Vector3(0.7144f, -0.028f, -2);

                viewSettingsInfoPanelRoleVariant.titleText.color =
                    viewSettingsInfoPanelRoleVariant.chanceTitle.color =
                        viewSettingsInfoPanelRoleVariant.chanceBackground.color =
                            viewSettingsInfoPanelRoleVariant.background.color =
                                customRole.RoleColor.FindAlternateColor();
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

    public static void SetModdedHeader(this CategoryHeaderRoleVariant header, StringNames roleName, int maskLayer, ModdedRoleTeams team, Sprite? roleIcon = null)
    {
        header.SetHeader(roleName, maskLayer);
        if (team is ModdedRoleTeams.Crewmate)
        {
            header.Background.color = Palette.CrewmateRoleHeaderBlue;
            header.Divider.color = Palette.CrewmateRoleHeaderBlue;
            header.Title.color = Palette.CrewmateRoleHeaderTextBlue;
        }
        else if (team is ModdedRoleTeams.Impostor)
        {
            header.Background.color = Palette.ImpostorRoleHeaderRed;
            header.Divider.color = Palette.ImpostorRoleHeaderRed;
            header.Title.color = Palette.ImpostorRoleHeaderTextRed;
        }
        else
        {
            header.Background.color = Color.grey;
            header.Divider.color = Color.grey;
            header.Title.color = new Color32(50, 50, 50, 255);
        }
        if (roleIcon != null && header.icon != null)
        {
            header.icon.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);
            header.icon.sprite = roleIcon;
        }
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

        var role = SelectedMod.InternalRoles.Values.FirstOrDefault(x => x.GetType() == roleType);

        if (role == null)
        {
            return 0;
        }

        if (role is not ICustomRole customRole)
        {
            return 0;
        }

        viewPanel.header.SetModdedHeader(
            role.StringName,
            maskLayer,
            customRole.Team,
            customRole.Configuration.Icon != null ? customRole.Configuration.Icon.LoadAsset() : MiraAssets.Empty.LoadAsset());
        viewPanel.header.icon.transform.localScale = new Vector3(0.465f, 0.465f, 1f);
        viewPanel.divider.material.SetInt(PlayerMaterial.MaskLayer, maskLayer);

        var num = viewPanel.yPosStart;
        var num2 = 1.08f;

        var filteredOptions = SelectedMod.InternalOptionGroups
            .Where(x => x.OptionableType == roleType)
            .SelectMany(x => x.Options)
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
