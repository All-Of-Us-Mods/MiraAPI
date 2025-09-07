using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.GameOptions;
using MiraAPI.Networking;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
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
    public static Dictionary<int, Vector3> RolePositions { get; set; } = [];

    private static Dictionary<RoleOptionsGroup, bool> RoleGroupHidden { get; set; } = [];
    private static List<GameObject> Headers { get; set; } = [];
    private static List<RoleOptionSetting> RoleOptionSettings { get; set; } = [];

    private static float ScrollerNum { get; set; } = 0.522f;

    private static RoleBehaviour? CurrentRole { get; set; }
    private static List<IModdedOption>? CurrentRoleOptions { get; set; }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(RolesSettingsMenu.SetQuotaTab))]
    public static void SetQuotaTabPostfix(RolesSettingsMenu __instance)
    {
        __instance.scrollBar.SetScrollBounds(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RolesSettingsMenu.SetQuotaTab))]
    public static bool SetQuotaTabPatch(RolesSettingsMenu __instance)
    {
        Headers.ForEach(Object.Destroy);
        RoleOptionSettings.ForEach(Object.Destroy);
        Headers.Clear();
        RoleOptionSettings.Clear();
        CurrentRole = null;
        CurrentRoleOptions = null;

        __instance.roleChances = new Il2CppSystem.Collections.Generic.List<RoleOptionSetting>();
        __instance.advancedSettingChildren = new Il2CppSystem.Collections.Generic.List<OptionBehaviour>();

        var maskBg = __instance.scrollBar.transform.FindChild("MaskBg");
        var hitbox = __instance.scrollBar.transform.FindChild("Hitbox");

        if (GameSettingMenuPatches.SelectedModIdx == 0)
        {
            __instance.AllButton.transform.parent.gameObject.SetActive(true);
            __instance.AllButton.gameObject.SetActive(true);
            __instance.scrollBar.transform.localPosition = new Vector3(-1.4957f, 0.657f, -4);
            maskBg.localPosition = new Vector3(1.5353f, -.5734f, -.1f);
            maskBg.localScale = new Vector3(6.6811f, 3.3563f, 0.5598f);
            hitbox.localPosition = new Vector3(0.3297f, -.2333f, 4f);
            hitbox.localScale = new Vector3(1, 1, 1);
            return true;
        }

        ScrollerNum = 0.522f;

        __instance.AllButton.transform.parent.gameObject.SetActive(false);
        __instance.AllButton.gameObject.SetActive(false);
        __instance.scrollBar.transform.localPosition = new Vector3(-1.4957f, 1.5261f, -4);
        maskBg.localPosition = new Vector3(1.5353f, -1.0607f, -.1f);
        maskBg.localScale = new Vector3(6.6811f, 4.1563f, 0.5598f);
        hitbox.localPosition = new Vector3(0.3297f, -.6333f, 4f);
        hitbox.localScale = new Vector3(1, 1.2f, 1);

        var num3 = 0;

        var roleGroups = GameSettingMenuPatches.SelectedMod?.InternalRoles.Values.OfType<ICustomRole>()
            .ToLookup(x => x.RoleOptionsGroup);

        if (roleGroups is null)
        {
            return true;
        }

        // sort the groups by priority
        var sortedRoleGroups = roleGroups
            .OrderBy(x => x.Key.Priority)
            .ThenBy(x => x.Key.Name);

        var quotaThing = __instance.categoryHeaderEditRoleOrigin.transform.FindChild("QuotaHeader");
        var template = __instance.transform.parent.parent.GetComponent<GameSettingMenu>().GameSettingsTab.categoryHeaderOrigin;

        foreach (var grouping in sortedRoleGroups)
        {
            if (!grouping.Any() || grouping.All(x=>x.Configuration.HideSettings))
            {
                continue;
            }

            var group = grouping.Key;

            RoleGroupHidden.TryAdd(group, false);

            var name = group.Name switch
            {
                "Crewmate" => StringNames.CrewmateRolesHeader,
                "Impostor" => StringNames.ImpostorRolesHeader,
                _ => CustomStringName.CreateAndRegister(group.Name),
            };

            var categoryHeaderMasked = Object.Instantiate(
                template,
                Vector3.zero,
                Quaternion.identity,
                __instance.RoleChancesSettings.transform);

            categoryHeaderMasked.SetHeader(name, 20);

            var quotaInst = Object.Instantiate(quotaThing, categoryHeaderMasked.transform);
            quotaInst.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
            quotaInst.transform.localPosition = new Vector3(0.7f, -0.82f, 0);

            var chanceText = quotaInst.transform.FindChild("Chance Text");
            chanceText.transform.localPosition = new Vector3(4.3f, 0.0993f, 0);

            var countText = quotaInst.transform.FindChild("# Text");
            countText.transform.localPosition = new Vector3(1.9f, 0.0993f, 0f);

            var blankLabel = quotaInst.transform.FindChild("BlankLabel").gameObject;
            var chanceLabel = quotaInst.transform.FindChild("Chance Label").gameObject;
            var countLabel = quotaInst.transform.FindChild("# Label").gameObject;
            blankLabel.Destroy();
            chanceLabel.Destroy();
            countLabel.Destroy();

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
                    categoryHeaderMasked.Title.color = group.Color.Equals(MiraApiPlugin.DefaultHeaderColor) ? Color.white : group.Color.FindAlternateColor();
                    categoryHeaderMasked.Divider.color = group.Color;
                    categoryHeaderMasked.Background.color = group.Color;
                    break;
            }

            categoryHeaderMasked.Title.fontStyle = __instance.categoryHeaderEditRoleOrigin.Title.fontStyle;
            categoryHeaderMasked.Title.font = __instance.categoryHeaderEditRoleOrigin.Title.font;
            categoryHeaderMasked.Title.fontMaterial = __instance.categoryHeaderEditRoleOrigin.Title.fontMaterial;

            categoryHeaderMasked.Divider.color = categoryHeaderMasked.Background.color;
            categoryHeaderMasked.Background.transform.localPosition = new Vector3(0.55f, -0.1833f, 0);
            categoryHeaderMasked.Background.size = new Vector2(categoryHeaderMasked.Background.size.x + 1.5f, categoryHeaderMasked.Background.size.y);

            categoryHeaderMasked.transform.localScale = Vector3.one * 0.63f;
            categoryHeaderMasked.transform.localPosition = new Vector3(-0.44f, ScrollerNum, -2f);
            categoryHeaderMasked.gameObject.SetActive(true);
            quotaInst.gameObject.SetActive(!RoleGroupHidden[group]);

            var label = RoleGroupHidden[group]
                ? "(Click to open)"
                : "(Click to close)";
            var newText = Object.Instantiate(categoryHeaderMasked.Title, categoryHeaderMasked.transform);
            newText.text = $"<size=70%>{label}</size>";
            newText.transform.localPosition = new Vector3(2.6249f, -0.165f, 0f);
            newText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();

            Headers.Add(categoryHeaderMasked.gameObject);
            ScrollerNum -= 0.422f;

            if (!RoleGroupHidden[group])
            {
                foreach (var role in grouping)
                {
                    if (role is not RoleBehaviour roleBehaviour)
                    {
                        continue;
                    }

                    var option = CreateQuotaOption(__instance, roleBehaviour, num3);
                    if (option is not null)
                    {
                        RoleOptionSettings.Add(option);
                        num3++;
                    }
                }
            }

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
                    RolePositions[GameSettingMenuPatches.SelectedModIdx] = __instance.scrollBar.Inner.localPosition;
                    if (RoleGroupHidden.TryGetValue(group, out var groupHidden))
                    {
                        RoleGroupHidden[group] = !groupHidden;
                    }
                    foreach (var header in Headers)
                    {
                        header.Destroy();
                    }
                    Headers.Clear();
                    foreach (var option in RoleOptionSettings)
                    {
                        option.gameObject.Destroy();
                    }
                    RoleOptionSettings.Clear();
                    __instance.SetQuotaTab();
                }));
            headerBtn.SetButtonEnableState(true);

            if (RoleGroupHidden.TryGetValue(group, out var value) && !value)
            {
                ScrollerNum -= 0.4f;
            }
        }

        __instance.scrollBar.SetScrollBounds(__instance);
        return false;
    }

    private static void SetScrollBounds(this Scroller scroller, RolesSettingsMenu roleMenu)
    {
        if (GameSettingMenuPatches.SelectedModIdx == 0)
        {
            scroller.CalculateAndSetYBounds(roleMenu.roleChances.Count + 3, 1f, 6f, 0.43f);
            return;
        }

        scroller.SetYBoundsMax(-ScrollerNum - 2);
        if (RolePositions.TryGetValue(GameSettingMenuPatches.SelectedModIdx, out var scroll))
        {
            scroller.Inner.localPosition = scroll;
            scroller.UpdateScrollBars();
        }
        else
        {
            scroller.ScrollToTop();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RolesSettingsMenu.Update))]
    public static bool UpdatePatch(RolesSettingsMenu __instance)
    {
        if (GameSettingMenuPatches.SelectedModIdx == 0) return true;
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

    [HarmonyPostfix]
    [HarmonyPatch(nameof(RolesSettingsMenu.OpenChancesTab))]
    public static void OpenChancesTabPostfix(RolesSettingsMenu __instance)
    {
        if (!__instance.scrollBar)
        {
            return;
        }

        CurrentRole = null;
        CurrentRoleOptions = null;
        __instance.scrollBar.SetScrollBounds(__instance);
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
            Logger<MiraApiPlugin>.Warning(e);
        }

        roleSetting.UpdateValuesAndText(GameOptionsManager.Instance.CurrentGameOptions.RoleOptions);
        HudManager.Instance.Notifier.AddRoleSettingsChangeMessage(
            roleSetting.Role.StringName,
            roleSetting.RoleMaxCount,
            roleSetting.RoleChance,
            roleSetting.Role.TeamType,
            false);

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
            optBehaviour.gameObject.DestroyImmediate();
        }

        CurrentRole = role;
        __instance.advancedSettingChildren.Clear();

        // TODO: create sub groups under the role settings.
        var filteredOptions = GameSettingMenuPatches.SelectedMod?.InternalOptionGroups
            .Where(x => x.GroupVisible() && x.OptionableType == role.GetType())
            .SelectMany(x => x.Options)
            .ToList() ?? [];

        CurrentRoleOptions = filteredOptions;

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

        __instance.scrollBar.ScrollToTop();
    }

    private static void ChangeTab(RoleBehaviour role, RolesSettingsMenu __instance)
    {
        if (role is not ICustomRole customRole)
        {
            Logger<MiraApiPlugin>.Error($"Role {role.NiceName} is not a custom role.");
            return;
        }

        RolePositions[GameSettingMenuPatches.SelectedModIdx] = __instance.scrollBar.Inner.localPosition;

        __instance.roleDescriptionText.text = customRole.RoleLongDescription;
        __instance.roleTitleText.text = TranslationController.Instance.GetString(
            role.StringName,
            new Il2CppReferenceArray<Il2CppSystem.Object>(0));

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

            __instance.roleScreenshot.sprite = Sprite.Create(
                role.RoleScreenshot.texture,
                new Rect(0, 0, 370, 230),
                Vector2.one / 2,
                100);
            __instance.roleScreenshot.drawMode = SpriteDrawMode.Sliced;
        }

        __instance.roleHeaderSprite.color = customRole.OptionsMenuColor;
        __instance.roleHeaderText.color = customRole.OptionsMenuColor.FindAlternateColor();

        var categoryHeaderMasked = __instance.AdvancedRolesSettings.transform.Find("CategoryHeaderMasked").GetComponent<CategoryHeaderMasked>();

        if (categoryHeaderMasked.Title.gameObject.TryGetComponent<TextTranslatorTMP>(out var comp))
        {
            comp.Destroy();
        }

        categoryHeaderMasked.Title.text = "RETURN TO ROLE SETTINGS";

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
                    __instance.OpenChancesTab();
                }));
            headerBtn.SetButtonEnableState(true);
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
        __instance.RefreshChildren();
    }

    private static RoleOptionSetting? CreateQuotaOption(RolesSettingsMenu __instance, RoleBehaviour role, int index)
    {
        if (role is not ICustomRole customRole)
        {
            Logger<MiraApiPlugin>.Error($"Role {role.NiceName} is not a custom role.");
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

        if (GameSettingMenuPatches.SelectedMod is null ||
            GameSettingMenuPatches.SelectedMod.InternalOptionGroups
                .Exists(x => x.GroupVisible() && x.OptionableType == role.GetType() && x.Options.Any(y => y.Visible())))
        {
            var newButton = Object.Instantiate(roleOptionSetting.buttons[0], roleOptionSetting.transform);
            newButton.name = "ConfigButton";
            newButton.transform.localPosition = new Vector3(0.4473f, -0.3f, -2f);
            newButton.transform.FindChild("Text_TMP").gameObject.DestroyImmediate();
            newButton.activeSprites.Destroy();

            var btnRend = newButton.transform.FindChild("ButtonSprite").GetComponent<SpriteRenderer>();
            btnRend.sprite = MiraAssets.Cog.LoadAsset();

            var passiveButton = newButton.GetComponent<GameOptionButton>();
            passiveButton.OnClick = new ButtonClickedEvent();
            passiveButton.interactableColor = btnRend.color = customRole.OptionsMenuColor.FindAlternateColor();
            passiveButton.interactableHoveredColor = Color.white;

            passiveButton.OnClick.AddListener((UnityAction)(() => { ChangeTab(role, __instance); }));

            if (customRole.Configuration.Icon != null)
            {
                var roleIcon = new GameObject("RoleIcon");
                roleIcon.transform.parent = roleOptionSetting.transform;
                roleIcon.transform.localScale = new(.25f, .25f, 1);
                roleIcon.layer = LayerMask.NameToLayer("UI");
                roleIcon.transform.localPosition = new Vector3(-1.3f, -0.3f, -2f);
                var rend = roleIcon.AddComponent<SpriteRenderer>();
                rend.sprite = customRole.Configuration.Icon.LoadAsset();
                rend.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            }
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

        if (index < GameSettingMenuPatches.SelectedMod?.InternalRoles.Count - 1)
        {
            ScrollerNum -= 0.43f;
        }

        return roleOptionSetting;
    }
}
