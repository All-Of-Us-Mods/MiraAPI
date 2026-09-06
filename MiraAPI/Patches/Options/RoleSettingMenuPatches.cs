using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(RolesSettingsMenu))]
public static class RoleSettingMenuPatches
{
    private static Dictionary<RoleOptionsGroup, bool> RoleGroupHidden { get; set; } = [];

    private static float ScrollerNum { get; set; } = 0.522f;

    private static RoleBehaviour? CurrentRole { get; set; }
    private static List<IModdedOption>? CurrentRoleOptions { get; set; }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(RolesSettingsMenu.OnEnable))]
    public static void OpenPatch()
    {
        HudManager.Instance.PlayerCam.OverrideScreenShakeEnabled = false;
    }

    private static IEnumerator CoQuotaTabPatch(RolesSettingsMenu roleMenu)
    {
        CurrentRole = null;
        CurrentRoleOptions = null;

        roleMenu.QuotaTabSelectables.Clear();
        roleMenu.roleChances.Clear();
        roleMenu.advancedSettingChildren.Clear();

        var maskBg = roleMenu.scrollBar.transform.FindChild("MaskBg");
        var hitbox = roleMenu.scrollBar.transform.FindChild("Hitbox");
        var dividerImage = roleMenu.transform.FindChild("HeaderButtons/DividerImage");

        if (MenuState.Instance.CurrentModIdx == 0)
        {
            roleMenu.AllButton.transform.parent.gameObject.SetActive(true);
            roleMenu.AllButton.gameObject.SetActive(true);
            roleMenu.scrollBar.transform.localPosition = new Vector3(-1.4957f, 0.657f, -4);
            maskBg.localPosition = new Vector3(1.5353f, -.5734f, -.1f);
            maskBg.localScale = new Vector3(6.6811f, 3.3563f, 0.5598f);
            hitbox.localPosition = new Vector3(0.3297f, -.2333f, 4f);
            hitbox.localScale = new Vector3(1, 1, 1);
            dividerImage.gameObject.SetActive(true);
        }
        else
        {
            roleMenu.AllButton.transform.parent.gameObject.SetActive(false);
            roleMenu.AllButton.gameObject.SetActive(false);
            roleMenu.scrollBar.transform.localPosition = new Vector3(-1.4957f, 1.5261f, -4);
            maskBg.localPosition = new Vector3(1.5353f, -1.0607f, -.1f);
            maskBg.localScale = new Vector3(6.6811f, 4.1563f, 0.5598f);
            hitbox.localPosition = new Vector3(0.3297f, -.6333f, 4f);
            hitbox.localScale = new Vector3(1, 1.2f, 1);
            dividerImage.gameObject.SetActive(false);
        }

        var queuedRefresh = false;
        if (MenuState.Instance.QueuedRoleMenuRefresh.TryGetValue(MenuState.Instance.CurrentModIdx, out var queued) && queued)
        {
            queuedRefresh = true;
            var roleOptionSettings = roleMenu.scrollBar.Inner.GetComponentsInChildren<RoleOptionSetting>(true);
            var headers = roleMenu.scrollBar.Inner.GetComponentsInChildren<CategoryHeaderMasked>(true);
            foreach (var child in roleOptionSettings)
            {
                Object.Destroy(child.gameObject);
            }
            foreach (var child in headers)
            {
                Object.Destroy(child.gameObject);
            }
            yield return new WaitForEndOfFrame();
            MenuState.Instance.QueuedRoleMenuRefresh[MenuState.Instance.CurrentModIdx] = false;
        }
        if (!queuedRefresh && MenuState.Instance.FinishedRoleMenus.TryGetValue(MenuState.Instance.CurrentModIdx, out var finished) && finished)
        {
            var roleOptionSettings = roleMenu.scrollBar.Inner.GetComponentsInChildren<RoleOptionSetting>(true);
            foreach (var r in roleOptionSettings)
            {
                roleMenu.roleChances.Add(r);
                roleMenu.QuotaTabSelectables.AddRange(new Il2CppSystem.Collections.Generic.IEnumerable<UiElement>(r.ControllerSelectable.Pointer));
            }

            Info($"Already created role options for {MenuState.Instance.CurrentModIdx}");
        }
        else
        {
            if (MenuState.Instance.CurrentModIdx == 0)
            {
                var num = 0.662f;

                var list = CustomRoleManager.AllRoles.Where(r =>
                    !r.IsCustomRole() && !r.IsRoleBlacklisted() && r.TeamType == RoleTeamTypes.Crewmate &&
                    r.Role != RoleTypes.Crewmate &&
                    r.Role != RoleTypes.CrewmateGhost).ToList();
                var list2 = CustomRoleManager.AllRoles.Where(r =>
                    !r.IsCustomRole() && !r.IsRoleBlacklisted() && r.TeamType == RoleTeamTypes.Impostor &&
                    r.Role != RoleTypes.Impostor &&
                    r.Role != RoleTypes.ImpostorGhost).ToList();

                if (roleMenu.roleTabs == null || roleMenu.roleTabs.Count == 0)
                {
                    var num2 = -1.928f;
                    roleMenu.roleTabs = new();
                    roleMenu.roleTabs.Add(roleMenu.AllButton);
                    foreach (var t in list)
                    {
                        roleMenu.AddRoleTab(t, ref num2);
                    }

                    foreach (var t in list2)
                    {
                        roleMenu.AddRoleTab(t, ref num2);
                    }
                }

                var categoryHeaderEditRole = Object.Instantiate(
                    roleMenu.categoryHeaderEditRoleOrigin,
                    Vector3.zero,
                    Quaternion.identity,
                    roleMenu.RoleChancesSettings.transform);
                categoryHeaderEditRole.SetHeader(StringNames.CrewmateRolesHeader, 20);
                categoryHeaderEditRole.transform.localPosition = new Vector3(4.986f, num, -2f);
                num -= 0.522f;
                var num3 = 0;
                foreach (var t in list)
                {
                    roleMenu.CreateQuotaOption(t, ref num, num3);
                    num3++;
                }

                num -= 0.22f;
                var categoryHeaderEditRole2 = Object.Instantiate(
                    roleMenu.categoryHeaderEditRoleOrigin,
                    Vector3.zero,
                    Quaternion.identity,
                    roleMenu.RoleChancesSettings.transform);
                categoryHeaderEditRole2.SetHeader(StringNames.ImpostorRolesHeader, 20);
                categoryHeaderEditRole2.transform.localPosition = new Vector3(4.986f, num, -2f);
                num -= 0.522f;
                foreach (var t in list2)
                {
                    roleMenu.CreateQuotaOption(t, ref num, num3);
                    num3++;
                }

                Info($"Created {num3} role options for the default game.");
            }
            else
            {
                ScrollerNum = 0.522f;

                var num4 = 0;
                var roleOptionSettings = roleMenu.scrollBar.Inner.GetComponentsInChildren<RoleOptionSetting>(false);
                var headers = roleMenu.scrollBar.Inner.GetComponentsInChildren<CategoryHeaderMasked>(false);
                foreach (var child in roleOptionSettings)
                {
                    Object.Destroy(child.gameObject);
                }
                foreach (var child in headers)
                {
                    Object.Destroy(child.gameObject);
                }
                yield return new WaitForEndOfFrame();

                var roleGroups = MenuState.Instance.CurrentMod.InternalRoles.Values.OfType<ICustomRole>()
                    .ToLookup(x => x.RoleOptionsGroup);

                if (roleGroups.Count == 0)
                {
                    Error("No role groups found for the selected mod.");
                }
                else
                {
                    // sort the groups by priority
                    var sortedRoleGroups = roleGroups
                        .OrderBy(x => x.Key.Priority)
                        .ThenBy(x => x.Key.Name.Translate());

                    var quotaThing = roleMenu.categoryHeaderEditRoleOrigin.transform.FindChild("QuotaHeader");
                    var usingNewQuota = false;
                    var template = roleMenu.transform.parent.parent.GetComponent<GameSettingMenu>().GameSettingsTab
                        .categoryHeaderOrigin;

                    foreach (var grouping in sortedRoleGroups)
                    {
                        if (!grouping.Any() ||
                            grouping.All(x => x.Configuration.HideSettings || !x.VisibleInSettings() || !x.Configuration.AssociatedGameMode.IsInstanceOfType(CustomGameModeManager.ActiveMode)))
                        {
                            continue;
                        }

                        var group = grouping.Key;

                        RoleGroupHidden.TryAdd(group, false);

                        var name = MiraLocaleManager.GetOrCreateLocaleString(group.Name);

                        var categoryHeaderMasked = Object.Instantiate(
                            template,
                            Vector3.zero,
                            Quaternion.identity,
                            roleMenu.RoleChancesSettings.transform);

                        categoryHeaderMasked.SetHeader(name, 20);

                        var quotaInst = Object.Instantiate(quotaThing, categoryHeaderMasked.transform);
                        quotaInst.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
                        quotaInst.transform.localPosition = new Vector3(0.7f, -0.82f, 0);

                        var chanceText = quotaInst.transform.FindChild("Chance Text");
                        chanceText.transform.localPosition = new Vector3(4.3f, 0.0993f, 0);

                        var countText = quotaInst.transform.FindChild("# Text");
                        countText.transform.localPosition = new Vector3(1.9f, 0.0993f, 0f);

                        if (!usingNewQuota)
                        {
                            var blankLabel = quotaInst.transform.FindChild("BlankLabel").gameObject;
                            var chanceLabel = quotaInst.transform.FindChild("Chance Label").gameObject;
                            var countLabel = quotaInst.transform.FindChild("# Label").gameObject;
                            blankLabel.Destroy();
                            chanceLabel.Destroy();
                            countLabel.Destroy();
                            usingNewQuota = true;
                            quotaThing = quotaInst;
                        }

                        categoryHeaderMasked.Background.sprite = MiraAssets.CategoryHeader.LoadAsset();
                        categoryHeaderMasked.Background.sprite.texture.filterMode = FilterMode.Bilinear;
                        categoryHeaderMasked.Background.sprite.texture.wrapMode = TextureWrapMode.Clamp;

                        categoryHeaderMasked.Background.transform.localPosition = new Vector3(0.5f, -0.1833f, 0);

                        switch (name)
                        {
                            case StringNames.CrewmateRolesHeader:
                                categoryHeaderMasked.Title.color = Palette.CrewmateRoleHeaderTextBlue;
                                categoryHeaderMasked.Background.color = Palette.CrewmateRoleHeaderBlue;
                                break;
                            case StringNames.ImpostorRolesHeader:
                                categoryHeaderMasked.Title.color = Palette.ImpostorRoleHeaderTextRed;
                                categoryHeaderMasked.Background.color = Palette.ImpostorRoleHeaderRed;
                                break;
                            default:
                                categoryHeaderMasked.Title.color = group.Color.Equals(MiraApiPlugin.DefaultHeaderColor)
                                    ? Color.white
                                    : group.Color.FindAlternateColor();
                                categoryHeaderMasked.Divider.color = group.Color;
                                categoryHeaderMasked.Background.color = group.Color;
                                break;
                        }

                        categoryHeaderMasked.Title.fontStyle = roleMenu.categoryHeaderEditRoleOrigin.Title.fontStyle;
                        categoryHeaderMasked.Title.font = roleMenu.categoryHeaderEditRoleOrigin.Title.font;
                        categoryHeaderMasked.Title.fontMaterial =
                            roleMenu.categoryHeaderEditRoleOrigin.Title.fontMaterial;

                        categoryHeaderMasked.Divider.color = categoryHeaderMasked.Background.color;
                        categoryHeaderMasked.Background.transform.localPosition = new Vector3(0.55f, -0.1833f, 0);
                        categoryHeaderMasked.Background.size = new Vector2(
                            categoryHeaderMasked.Background.size.x + 1.5f,
                            categoryHeaderMasked.Background.size.y);

                        categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
                        categoryHeaderMasked.transform.localPosition = new Vector3(-0.44f, ScrollerNum, -2f);
                        categoryHeaderMasked.gameObject.SetActive(true);
                        quotaInst.gameObject.SetActive(!RoleGroupHidden[group]);

                        var label = RoleGroupHidden[group]
                            ? $"({"MiraApi.GameSetting.Global.ClickToOpen".Translate()})"
                            : $"({"MiraApi.GameSetting.Global.ClickToClose".Translate()})";
                        var newText = Object.Instantiate(categoryHeaderMasked.Title, categoryHeaderMasked.transform);
                        newText.text = $"<size=70%>{label}</size>";
                        newText.transform.localPosition = new Vector3(2.6249f, -0.165f, 0f);
                        newText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();

                        ScrollerNum -= 0.422f;

                        if (!RoleGroupHidden[group])
                        {
                            yield return
                                grouping.CoLoopWithBudget(role =>
                                {
                                    if (role is not RoleBehaviour roleBehaviour)
                                    {
                                        return;
                                    }

                                    var option = CreateQuotaOption(roleMenu, roleBehaviour, num4);
                                    if (option is not null)
                                    {
                                        num4++;
                                    }
                                });
                        }

                        var boxCol = categoryHeaderMasked.gameObject.AddComponent<BoxCollider2D>();
                        boxCol.size = new Vector2(7, 0.7f);
                        boxCol.offset = new Vector2(1.5f, -0.3f);

                        var headerBtn = categoryHeaderMasked.gameObject.AddComponent<PassiveButton>();
                        headerBtn.ClickSound = roleMenu.BackButton.GetComponent<PassiveButton>().ClickSound;
                        headerBtn.OnMouseOver = new UnityEvent();
                        headerBtn.OnMouseOut = new UnityEvent();
                        headerBtn.OnClick.AddListener(
                            (UnityAction)(() =>
                            {
                                if (RoleGroupHidden.TryGetValue(group, out var groupHidden))
                                {
                                    RoleGroupHidden[group] = !groupHidden;
                                }

                                MenuState.Instance.QueuedRoleMenuRefresh[MenuState.Instance.CurrentModIdx] = false;
                                MenuState.Instance.FinishedRoleMenus[MenuState.Instance.CurrentModIdx] = false;

                                foreach (var child in roleMenu.RoleChancesSettings.transform)
                                {
                                    var transform = child.Cast<Transform>();
                                    Object.Destroy(transform.gameObject);
                                }

                                roleMenu.OpenChancesTab();
                            }));
                        headerBtn.SetButtonEnableState(true);

                        if (RoleGroupHidden.TryGetValue(group, out var value) && !value)
                        {
                            ScrollerNum -= 0.4f;
                        }
                    }
                }
            }
            MenuState.Instance.QueuedRoleMenuRefresh[MenuState.Instance.CurrentModIdx] = false;
            MenuState.Instance.FinishedRoleMenus[MenuState.Instance.CurrentModIdx] = true;
        }

        roleMenu.SetScrollBounds();
        _quotaTabCoroutine = null;
    }

    private static Coroutine? _quotaTabCoroutine;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RolesSettingsMenu.OpenChancesTab))]
    public static bool OpenChancesTabPatch(RolesSettingsMenu __instance, bool controllerSelected)
    {
        __instance.selectedRoleTab = 0;
        __instance.RoleChancesSettings.SetActive(true);
        __instance.AdvancedRolesSettings.SetActive(false);
        __instance.ControllerSelectable.Clear();
        if (_quotaTabCoroutine != null)
        {
            GameSettingMenu.Instance.StopCoroutine(_quotaTabCoroutine);
        }

        __instance.QuotaTabSelectables ??= new();
        __instance.roleChances ??= new();
        __instance.advancedSettingChildren ??= new();

        _quotaTabCoroutine = GameSettingMenu.Instance.StartCoroutine(CoOpenChancesTab().WrapToIl2Cpp());
        return false;

        IEnumerator CoOpenChancesTab()
        {
            yield return CoQuotaTabPatch(__instance);
            if (controllerSelected)
            {
                __instance.ControllerSelectable.AddRange(
                    new Il2CppSystem.Collections.Generic.IEnumerable<UiElement>(
                        __instance.QuotaTabSelectables.Pointer));
            }

            if (controllerSelected)
            {
                ControllerManager.Instance.CurrentUiState.SelectableUiElements = __instance.ControllerSelectable;
                if (__instance.ControllerSelectable.Count != 0)
                {
                    ControllerManager.Instance.SetDefaultSelection(__instance.ControllerSelectable[0]);
                }
            }
            var passiveButton = __instance.currentTabButton;
            if (passiveButton)
            {
                passiveButton.SelectButton(false);
            }
            __instance.AllButton.SelectButton(true);
            __instance.currentTabButton = __instance.AllButton;
        }
    }

    private static void SetScrollBounds(this RolesSettingsMenu menu, bool isRolesConfig = false)
    {
        var scroller = menu.scrollBar;

        if (MenuState.Instance.CurrentModIdx == 0)
        {
            scroller.CalculateAndSetYBounds(menu.roleChances.Count + 3, 1f, 6f, 0.43f);
            return;
        }

        scroller.Inner = isRolesConfig ? menu.AdvancedRolesSettings.transform : menu.RoleChancesSettings.transform;
        scroller.SetYBoundsMax(-ScrollerNum - 2);
    }

    [HarmonyPrefix]
    // TODO: turn this into a fixed update
    [HarmonyPatch(nameof(RolesSettingsMenu.Update))]
    public static bool UpdatePatch(RolesSettingsMenu __instance)
    {
        if (MenuState.Instance.CurrentModIdx == 0) return true;
        if (CurrentRole == null || CurrentRoleOptions == null) return false;

        var hasImage = CurrentRole.RoleScreenshot != null;
        var num = hasImage ? -0.872f : -1;
        foreach (var opt in CurrentRoleOptions)
        {
            if (opt.OptionBehaviour == null) continue;

            if (!opt.Visible.Invoke())
            {
                opt.OptionBehaviour.gameObject.SetActive(false);
                continue;
            }

            opt.OptionBehaviour.transform.localPosition = new Vector3(hasImage ? 2.17f : 1.1f, num, -2f);
            opt.OptionBehaviour.gameObject.SetActive(true);
            num += -0.45f;
        }

        __instance.scrollBar.SetYBoundsMax(-num - 3);
        return false;
    }

    private static void ValueChanged(OptionBehaviour obj)
    {
        var roleSetting = obj.Cast<RoleOptionSetting>();
        var role = roleSetting.Role as ICustomRole;
        if (role is null or { Configuration.HideSettings: true })
        {
            return;
        }

        try
        {
            if (role.Configuration.MaxRoleCount != 0)
            {
                role.SetCount(roleSetting.RoleMaxCount);
            }

            if (role.Configuration.CanModifyChance)
            {
                role.SetChance(roleSetting.RoleChance);
            }
        }
        catch (Exception e)
        {
            Warning(e);
        }

        roleSetting.UpdateValuesAndText(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);
        HudManager.Instance.Notifier.AddRoleSettingsChangeMessage(
            roleSetting.Role.StringName,
            roleSetting.RoleMaxCount,
            roleSetting.RoleChance,
            roleSetting.Role.TeamType);

        if (AmongUsClient.Instance.AmHost)
        {
            Rpc<SyncRoleOptionsRpc>.Instance.Send(PlayerControl.LocalPlayer, [role.GetNetData()], true);
        }

        GameOptionsManager.Instance.GameHostOptions = GameOptionsManager.Instance.CurrentGameOptions;
    }

    private static void CreateAdvancedSettings(RolesSettingsMenu __instance, RoleBehaviour role)
    {
        foreach (var optBehaviour in __instance.AdvancedRolesSettings.GetComponentsInChildren<OptionBehaviour>())
        {
            optBehaviour.gameObject.Destroy();
        }

        CurrentRole = role;
        __instance.advancedSettingChildren.Clear();

        // TODO: create sub groups under the role settings.
        var filteredOptions = MenuState.Instance.CurrentMod.InternalOptionGroups
            .Where(x => x.GroupVisible() && x.OptionableType == role.GetType())
            .SelectMany(x => x.Options)
            .ToList();

        foreach (var option in filteredOptions)
        {
            var newOpt = option.CreateOption(
                __instance.checkboxOrigin,
                __instance.numberOptionOrigin,
                __instance.stringOptionOrigin,
                GameSettingMenu.Instance.GameSettingsTab.playerOptionOrigin,
                __instance.AdvancedRolesSettings.transform);

            newOpt.SetClickMask(__instance.ButtonClickMask);

            SpriteRenderer[] componentsInChildren = newOpt.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in componentsInChildren)
            {
                renderer.material.SetInt(PlayerMaterial.MaskLayer, 20);
            }

            foreach (var fontMat in newOpt.GetComponentsInChildren<TextMeshPro>(true).Select(x => x.fontMaterial))
            {
                fontMat.SetFloat(ShaderID.StencilComp, 3f);
                fontMat.SetFloat(ShaderID.Stencil, 20);
            }

            newOpt.LabelBackground.enabled = false;
            __instance.advancedSettingChildren.Add(newOpt);

            newOpt.gameObject.SetActive(false);
            newOpt.Initialize();
        }

        CurrentRoleOptions = filteredOptions;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RolesSettingsMenu.ChangeTab))]
    public static void ChangeTabPatch(RolesSettingsMenu __instance)
    {
        var categoryHeaderMasked = __instance.AdvancedRolesSettings.transform.Find("CategoryHeaderMasked").GetComponent<CategoryHeaderMasked>();
        categoryHeaderMasked.Title.text = TranslationController.Instance.GetString(StringNames.RoleSettingsLabel);
        var labelBg = __instance.AdvancedRolesSettings.transform.FindChild("InfoLabelBackground");
        // ReSharper disable once StringLiteralTypo (Justification: Actual name in-game.)
        var imgBg = __instance.AdvancedRolesSettings.transform.FindChild("Imagebackground");
        imgBg.gameObject.SetActive(true);
        __instance.roleScreenshot.gameObject.SetActive(true);
        __instance.roleDescriptionText.transform.parent.localPosition = new Vector3(2.5176f, -0.2731f, -1f);
        __instance.roleDescriptionText.transform.parent.localScale = new Vector3(0.0675f, 0.1494f, 0.5687f);
        labelBg.transform.localPosition = new Vector3(1.082f, 0.1054f, -2.5f);
        __instance.roleScreenshot.drawMode = SpriteDrawMode.Simple;
        foreach (var optBehaviour in __instance.AdvancedRolesSettings.GetComponentsInChildren<OptionBehaviour>())
        {
            optBehaviour.gameObject.Destroy();
        }
    }

    private static void ChangeTab(RoleBehaviour role, RolesSettingsMenu __instance)
    {
        if (role is not ICustomRole customRole)
        {
            Error($"Role {role.NiceName} is not a custom role.");
            return;
        }

        __instance.roleDescriptionText.text = customRole.RoleMedDescription;
        __instance.roleTitleText.text = role.GetRoleName();

        // ReSharper disable once StringLiteralTypo (Justification: Actual name in-game.)
        var imgBg = __instance.AdvancedRolesSettings.transform.FindChild("Imagebackground");
        var labelBg = __instance.AdvancedRolesSettings.transform.FindChild("InfoLabelBackground");
        if (role.RoleScreenshot == null)
        {
            imgBg.gameObject.SetActive(false);
            __instance.roleScreenshot.gameObject.SetActive(false);
            __instance.roleDescriptionText.transform.parent.localPosition = new Vector3(1.5f, -0.2731f, -1);
            __instance.roleDescriptionText.transform.parent.localScale = new Vector3(0.09f, 0.2f, 0.5687f);
            labelBg.localPosition = new Vector3(-0.7f, 0.1054f, -2.5f);
        }
        else
        {
            imgBg.gameObject.SetActive(true);
            __instance.roleScreenshot.gameObject.SetActive(true);
            __instance.roleDescriptionText.transform.parent.localPosition = new Vector3(2.5176f, -0.2731f, -1f);
            __instance.roleDescriptionText.transform.parent.localScale = new Vector3(0.0675f, 0.1494f, 0.5687f);
            labelBg.transform.localPosition = new Vector3(1.082f, 0.1054f, -2.5f);

            __instance.roleScreenshot.sprite = role.RoleScreenshot;
            __instance.roleScreenshot.drawMode = SpriteDrawMode.Simple;
        }

        __instance.roleHeaderSprite.color = customRole.OptionsMenuColor;
        __instance.roleHeaderText.color = customRole.OptionsMenuColor.FindAlternateColor();

        var categoryHeaderMasked = __instance.AdvancedRolesSettings.transform.Find("CategoryHeaderMasked").GetComponent<CategoryHeaderMasked>();

        if (categoryHeaderMasked.Title.gameObject.TryGetComponent<TextTranslatorTMP>(out var comp))
        {
            comp.Destroy();
        }

        categoryHeaderMasked.Title.text = "MiraApi.ReturnToRoleSettings".Translate();

        if (!categoryHeaderMasked.gameObject.TryGetComponent<PassiveButton>(out _))
        {
            var boxCol = categoryHeaderMasked.gameObject.AddComponent<BoxCollider2D>();
            boxCol.size = new Vector2(7, 0.7f);
            boxCol.offset = new Vector2(1.5f, -0.3f);

            var headerBtn = categoryHeaderMasked.gameObject.AddComponent<PassiveButton>();
            headerBtn.ClickSound = __instance.BackButton.GetComponent<PassiveButton>().ClickSound;
            headerBtn.OnMouseOver = new UnityEvent();
            headerBtn.OnMouseOut = new UnityEvent();
            headerBtn.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    GameSettingMenu.Instance.StartCoroutine(CoReturnToRoleSettings().WrapToIl2Cpp());
                }));
            headerBtn.SetButtonEnableState(true);

            IEnumerator CoReturnToRoleSettings()
            {
                // set game objects
                __instance.RoleChancesSettings.SetActive(true);
                __instance.AdvancedRolesSettings.SetActive(false);

                // wait a frame
                yield return null;

                // re-enable page
                yield return CoQuotaTabPatch(__instance);

                // set controller selected button
                __instance.AllButton.SelectButton(true);
                __instance.currentTabButton = __instance.AllButton;
            }
        }

        var bg = __instance.AdvancedRolesSettings.transform.Find("Background");
        bg.localPosition = new Vector3(1.4041f, -7.08f, 0);
        bg.GetComponent<SpriteRenderer>().size = new Vector2(89.4628f, 100);

        CreateAdvancedSettings(__instance, role);

        foreach (var optionBehaviour in __instance.advancedSettingChildren)
        {
            if (optionBehaviour.IsCustom())
            {
                continue;
            }

            optionBehaviour.OnValueChanged = new Action<OptionBehaviour>(__instance.ValueChanged);
            if (AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
            {
                optionBehaviour.SetAsPlayer();
            }
        }

        __instance.RoleChancesSettings.SetActive(false);
        __instance.AdvancedRolesSettings.SetActive(true);
        __instance.SetScrollBounds(true);
        __instance.scrollBar.ScrollToTop();
        __instance.RefreshChildren();
    }

    private static RoleOptionSetting? CreateQuotaOption(RolesSettingsMenu __instance, RoleBehaviour role, int index)
    {
        if (role is not ICustomRole customRole)
        {
            Error($"Role {role.NiceName} is not a custom role.");
            return null;
        }

        if (customRole.Configuration.HideSettings)
        {
            return null;
        }

        var roleOptionSetting = Object.Instantiate(
            __instance.roleOptionSettingOrigin,
            Vector3.zero,
            Quaternion.identity,
            __instance.RoleChancesSettings.transform);
        roleOptionSetting.transform.localPosition = new Vector3(-0.1f, ScrollerNum, -2f);

        roleOptionSetting.SetRole(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions, role, 20);
        roleOptionSetting.titleText.text = role.GetRoleName();
        roleOptionSetting.labelSprite.color = customRole.OptionsMenuColor;
        roleOptionSetting.OnValueChanged = new Action<OptionBehaviour>(ValueChanged);
        roleOptionSetting.SetClickMask(__instance.ButtonClickMask);
        roleOptionSetting.ChanceMinusBtn.SetInteractable(true);
        roleOptionSetting.ChancePlusBtn.SetInteractable(true);
        roleOptionSetting.CountMinusBtn.SetInteractable(true);
        roleOptionSetting.CountPlusBtn.SetInteractable(true);
        __instance.roleChances.Add(roleOptionSetting);

        roleOptionSetting.titleText.transform.localPosition = new Vector3(-0.25f, -0.2923f, 0f);
        roleOptionSetting.titleText.color = customRole.OptionsMenuColor.FindAlternateColor();
        roleOptionSetting.titleText.horizontalAlignment = HorizontalAlignmentOptions.Left;

        if (customRole.Configuration.Icon != null)
        {
            var roleIcon = new GameObject("RoleIcon")
            {
                transform =
                {
                    parent = roleOptionSetting.transform,
                    localScale = new(.25f, .25f, 1),
                    localPosition = new Vector3(-1.3f, -0.3f, -2f),
                },
                layer = LayerMask.NameToLayer("UI"),
            };
            var rend = roleIcon.AddComponent<SpriteRenderer>();
            rend.sprite = customRole.Configuration.Icon.LoadAsset();

            rend.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        if (MenuState.Instance.CurrentMod.InternalOptionGroups
                .Exists(x => x.GroupVisible() && x.OptionableType == role.GetType() && x.Options.Any(y => y.Visible())))
        {
            var newButton = Object.Instantiate(roleOptionSetting.buttons[0], roleOptionSetting.transform);
            newButton.name = "ConfigButton";
            newButton.transform.localPosition = new Vector3(0.4473f, -0.3f, -2f);
            newButton.transform.FindChild("Text_TMP").gameObject.Destroy();
            newButton.activeSprites.Destroy();

            var btnRend = newButton.transform.FindChild("ButtonSprite").GetComponent<SpriteRenderer>();
            btnRend.sprite = MiraAssets.Cog.LoadAsset();

            var passiveButton = newButton.GetComponent<GameOptionButton>();
            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.interactableColor = btnRend.color = customRole.OptionsMenuColor.FindAlternateColor();
            passiveButton.interactableHoveredColor = Color.white;

            passiveButton.OnClick.AddListener((UnityAction)(() => { ChangeTab(role, __instance); }));
        }

        if (customRole.Configuration is { MaxRoleCount: 0 })
        {
            roleOptionSetting.CountMinusBtn.gameObject.SetActive(false);
            roleOptionSetting.CountPlusBtn.gameObject.SetActive(false);
        }

        if (!customRole.Configuration.CanModifyChance)
        {
            roleOptionSetting.ChanceMinusBtn.gameObject.SetActive(false);
            roleOptionSetting.ChancePlusBtn.gameObject.SetActive(false);
        }

        if (index < MenuState.Instance.CurrentMod.InternalRoles.Count - 1)
        {
            ScrollerNum -= 0.43f;
        }

        return roleOptionSetting;
    }
}
