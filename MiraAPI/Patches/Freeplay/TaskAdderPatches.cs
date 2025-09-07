using System;
using System.Globalization;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Freeplay;

[HarmonyPatch(typeof(TaskAdderGame))]
internal static class TaskAdderPatches
{
    private static Scroller _scroller = null!;
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TaskAdderGame.Begin))]
    public static void AddRolesFolder(TaskAdderGame __instance)
    {
        GameObject inner = new("Inner");
        inner.transform.SetParent(__instance.TaskParent.transform, false);

        _scroller = __instance.TaskParent.gameObject.AddComponent<Scroller>();
        _scroller.allowX = false;
        _scroller.allowY = true;
        _scroller.Inner = inner.transform;

        var scrollerHelper = __instance.TaskParent.gameObject.AddComponent<ManualScrollHelper>();
        scrollerHelper.scroller = _scroller;
        scrollerHelper.verticalAxis = RewiredConstsEnum.Action.TaskRVertical;
        scrollerHelper.scrollSpeed = 10f;

        GameObject hitbox = new("Hitbox")
        {
            layer = 5,
        };
        hitbox.transform.SetParent(__instance.TaskParent.transform, false);
        hitbox.transform.localScale = new Vector3(7.5f, 6.5f, 1);
        hitbox.transform.localPosition = new Vector3(2.8f, -2.2f, 0);

        var mask = hitbox.AddComponent<SpriteMask>();
        mask.sprite = MiraAssets.NextButton.LoadAsset();
        mask.alphaCutoff = 0.0f;

        var collider = hitbox.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1f, 1f);
        collider.enabled = true;

        _scroller.ClickMask = collider;

        __instance.TaskPrefab.GetComponent<PassiveButton>().ClickMask = collider;
        __instance.RoleButton.GetComponent<PassiveButton>().ClickMask = collider;
        __instance.RootFolderPrefab.GetComponent<PassiveButton>().ClickMask = collider;
        __instance.RootFolderPrefab.gameObject.SetActive(false);

        __instance.TaskParent = inner.transform;

        var rolesFolder = Object.Instantiate(__instance.RootFolderPrefab, _scroller.Inner);
        rolesFolder.gameObject.SetActive(false);
        rolesFolder.FolderName = "Roles";
        rolesFolder.name = "RolesFolder";

        var modifiersFolder = Object.Instantiate(__instance.RootFolderPrefab, _scroller.Inner);
        modifiersFolder.gameObject.SetActive(false);
        modifiersFolder.FolderName = "Modifiers";
        modifiersFolder.name = "ModifiersFolder";

        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
        {
            if (plugin.InternalRoles.Any(x => x.Value is ICustomRole { Configuration.ShowInFreeplay: true }))
            {
                var newFolder = Object.Instantiate(__instance.RootFolderPrefab, _scroller.Inner);
                newFolder.name = plugin.PluginId+":Roles";
                newFolder.FolderName = plugin.PluginInfo.Metadata.Name;
                newFolder.gameObject.SetActive(false);
                rolesFolder.SubFolders.Add(newFolder);
            }

            if (plugin.InternalModifiers.Exists(x => x.ShowInFreeplay))
            {
                var newFolder = Object.Instantiate(__instance.RootFolderPrefab, _scroller.Inner);
                newFolder.name = plugin.PluginId+":Modifiers";
                newFolder.FolderName = plugin.PluginInfo.Metadata.Name;
                newFolder.gameObject.SetActive(false);
                modifiersFolder.SubFolders.Add(newFolder);
            }
        }

        __instance.Root.SubFolders.Add(rolesFolder);
        __instance.Root.SubFolders.Add(modifiersFolder);

        __instance.GoToRoot();
    }

    private static void AddFileAsChildCustom(
        this TaskAdderGame instance,
        TaskAddButton item,
        ref float xCursor,
        ref float yCursor,
        ref float maxHeight)
    {
        item.transform.SetParent(instance.TaskParent);
        item.transform.localPosition = new Vector3(xCursor, yCursor, 0f);
        item.transform.localScale = Vector3.one;
        maxHeight = Mathf.Max(maxHeight, item.Text.bounds.size.y + 1.3f);
        xCursor += instance.fileWidth;
        if (xCursor > instance.lineWidth)
        {
            xCursor = 0f;
            yCursor -= maxHeight;
            maxHeight = 0f;
        }

        instance.ActiveItems.Add(item.transform);
    }

    // yes it might be crazy patching the entire method, but i tried so many other methods and only this works :cry:
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TaskAdderGame.ShowFolder))]
    public static bool ShowPatch(TaskAdderGame __instance, TaskFolder taskFolder)
    {
        var stringBuilder = new Il2CppSystem.Text.StringBuilder(64);
        __instance.Hierarchy.Add(taskFolder);
        for (var i = 0; i < __instance.Hierarchy.Count; i++)
        {
            stringBuilder.Append(__instance.Hierarchy.ToArray()[i].FolderName);
            stringBuilder.Append("\\");
        }

        __instance.PathText.text = stringBuilder.ToString();
        for (var j = 0; j < __instance.ActiveItems.Count; j++)
        {
            Object.Destroy(__instance.ActiveItems.ToArray()[j].gameObject);
        }

        __instance.ActiveItems.Clear();
        var num = 0f;
        var num2 = 0f;
        var num3 = 0f;
        for (var k = 0; k < taskFolder.SubFolders.Count; k++)
        {
            var taskFolder2 = Object.Instantiate(taskFolder.SubFolders.ToArray()[k], __instance.TaskParent);
            var folderTransform = taskFolder2.transform;

            taskFolder2.gameObject.SetActive(true);
            taskFolder2.Parent = __instance;

            folderTransform.localPosition = new Vector3(num, num2, 0f);
            folderTransform.localScale = Vector3.one;
            num3 = Mathf.Max(num3, taskFolder2.Text.bounds.size.y + 1.3f);
            num += __instance.folderWidth;
            if (num > __instance.lineWidth)
            {
                num = 0f;
                num2 -= num3;
                num3 = 0f;
            }

            __instance.ActiveItems.Add(folderTransform);
            if (!taskFolder2 || !taskFolder2.Button)
            {
                continue;
            }

            ControllerManager.Instance.AddSelectableUiElement(taskFolder2.Button);
            if (!string.IsNullOrEmpty(__instance.restorePreviousSelectionByFolderName) &&
                taskFolder2.FolderName.Equals(__instance.restorePreviousSelectionByFolderName, StringComparison.Ordinal))
            {
                __instance.restorePreviousSelectionFound = taskFolder2.Button;
            }
        }

        var flag = false;
        var list = taskFolder.Children.ToArray().OrderBy(t => t.TaskType).ToList();

        for (var l = 0; l < list.Count; l++)
        {
            var taskAddButton = Object.Instantiate(__instance.TaskPrefab);
            taskAddButton.MyTask = list.ToArray()[l];
            switch (taskAddButton.MyTask.TaskType)
            {
                case TaskTypes.DivertPower:
                    {
                        var targetSystem = taskAddButton.MyTask.Cast<DivertPowerTask>().TargetSystem;
                        taskAddButton.Text.text = TranslationController.Instance.GetString(
                            StringNames.DivertPowerTo,
                            TranslationController.Instance.GetString(targetSystem));
                        break;
                    }
                case TaskTypes.FixWeatherNode:
                    {
                        var nodeId = taskAddButton.MyTask.Cast<WeatherNodeTask>().NodeId;
                        taskAddButton.Text.text =
                            TranslationController.Instance.GetString(
                                StringNames.FixWeatherNode) + " " +
                            TranslationController.Instance.GetString(
                                WeatherSwitchGame.ControlNames[nodeId]);
                        break;
                    }
                default:
                    taskAddButton.Text.text =
                        TranslationController.Instance.GetString(taskAddButton.MyTask.TaskType);
                    break;
            }

            __instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);
            if (taskAddButton.Button == null)
            {
                continue;
            }

            ControllerManager.Instance.AddSelectableUiElement(taskAddButton.Button);
            if (__instance.Hierarchy.Count == 1 || flag)
            {
                continue;
            }

            var component =
                ControllerManager.Instance.CurrentUiState.CurrentSelection.GetComponent<TaskFolder>();
            if (component != null)
            {
                __instance.restorePreviousSelectionByFolderName = component.FolderName;
            }

            ControllerManager.Instance.SetDefaultSelection(taskAddButton.Button);
            flag = true;
        }

        if (taskFolder.FolderName == "Roles")
        {
            for (var m = 0; m < RoleManager.Instance.AllRoles.Length; m++)
            {
                var roleBehaviour = RoleManager.Instance.AllRoles[m];
                if (roleBehaviour.Role == RoleTypes.ImpostorGhost || roleBehaviour.Role == RoleTypes.CrewmateGhost ||
                    CustomRoleManager.CustomRoles.ContainsKey((ushort)roleBehaviour.Role))
                {
                    continue;
                }

                var taskAddButton2 = Object.Instantiate(__instance.RoleButton);
                taskAddButton2.SafePositionWorld = __instance.SafePositionWorld;
                taskAddButton2.Text.text = "Be_" + roleBehaviour.NiceName + ".exe";
                __instance.AddFileAsChildCustom(taskAddButton2, ref num, ref num2, ref num3);
                taskAddButton2.Role = roleBehaviour;

                if (taskAddButton2.Button == null)
                {
                    continue;
                }

                ControllerManager.Instance.AddSelectableUiElement(taskAddButton2.Button);
                switch (m)
                {
                    case 0 when __instance.restorePreviousSelectionFound != null:
                        ControllerManager.Instance.SetDefaultSelection(__instance.restorePreviousSelectionFound);
                        __instance.restorePreviousSelectionByFolderName = string.Empty;
                        __instance.restorePreviousSelectionFound = null;
                        break;
                    case 0:
                        ControllerManager.Instance.SetDefaultSelection(taskAddButton2.Button);
                        break;
                }
            }
        }

        if (taskFolder)
        {
            var split = taskFolder.name.Split(':');
            if (split.Length == 2)
            {
                var pluginId = split[0];
                var folderName = split[1];

                if (MiraPluginManager.GetPluginByGuid(pluginId) is { } plugin)
                {
                    if (folderName == "Roles(Clone)")
                    {
                        PluginRoles(__instance, plugin, ref num, ref num2, ref num3);
                    }
                    else if (folderName == "Modifiers(Clone)")
                    {
                        PluginModifiers(__instance, plugin, ref num, ref num2, ref num3);
                    }
                }
            }
        }

        foreach (var chip in __instance.ActiveItems)
        {
            chip.GetComponentInChildren<TextMeshPro>().EnableStencilMasking();
            chip.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        if (_scroller)
        {
            _scroller.CalculateAndSetYBounds(__instance.ActiveItems.Count, 6, 4.5f, 1.65f);
            _scroller.SetYBoundsMin(0.0f);
            _scroller.ScrollToTop();
        }

        if (__instance.Hierarchy.Count == 1)
        {
            ControllerManager.Instance.SetBackButton(__instance.BackButton);
            return false;
        }

        ControllerManager.Instance.SetBackButton(__instance.FolderBackButton);
        return false;
    }

    private static void PluginModifiers(
        TaskAdderGame instance,
        MiraPluginInfo plugin,
        ref float num,
        ref float num2,
        ref float num3)
    {
        for (var m = 0; m < plugin.InternalModifiers.Count; m++)
        {
            var modifier = plugin.InternalModifiers[m];
            if (!modifier.ShowInFreeplay)
            {
                continue;
            }

            var taskAddButton = Object.Instantiate(instance.RoleButton);
            taskAddButton.name = modifier.TypeId.ToString(NumberFormatInfo.InvariantInfo);
            taskAddButton.role = null;
            taskAddButton.MyTask = null;
            taskAddButton.SafePositionWorld = instance.SafePositionWorld;
            taskAddButton.Text.text = modifier.ModifierName;
            taskAddButton.Text.fontSizeMin = 1;
            taskAddButton.Text.EnableMasking();
            taskAddButton.FileImage.color = modifier.FreeplayFileColor;
            taskAddButton.RolloverHandler.OutColor = modifier.FreeplayFileColor;
            taskAddButton.Button.OnClick = new Button.ButtonClickedEvent();
            taskAddButton.Button.OnClick.AddListener((UnityEngine.Events.UnityAction)(() =>
            {
                var id = modifier.TypeId;
                if (PlayerControl.LocalPlayer.HasModifier(id))
                {
                    PlayerControl.LocalPlayer.RemoveModifier(id);
                    taskAddButton.Overlay.enabled = false;
                }
                else
                {
                    PlayerControl.LocalPlayer.AddModifier(id);
                    taskAddButton.Overlay.enabled = true;
                }
            }));

            if (modifier is TimedModifier timed)
            {
                taskAddButton.FileImage.sprite = MiraAssets.TimedModifierFile.LoadAsset();
                taskAddButton.Text.text += $"({timed.Duration}s)";
            }

            instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);

            ControllerManager.Instance.AddSelectableUiElement(taskAddButton.Button);
            switch (m)
            {
                case 0 when instance.restorePreviousSelectionFound != null:
                    ControllerManager.Instance.SetDefaultSelection(instance.restorePreviousSelectionFound);
                    instance.restorePreviousSelectionByFolderName = string.Empty;
                    instance.restorePreviousSelectionFound = null;
                    break;
                case 0:
                    ControllerManager.Instance.SetDefaultSelection(taskAddButton.Button);
                    break;
            }
        }
    }

    private static void PluginRoles(TaskAdderGame instance, MiraPluginInfo plugin, ref float num, ref float num2, ref float num3)
    {
        for (var m = 0; m < plugin.InternalRoles.Count; m++)
        {
            var roleBehaviour = plugin.InternalRoles.ElementAt(m).Value;
            if (roleBehaviour.Role == RoleTypes.ImpostorGhost || roleBehaviour.Role == RoleTypes.CrewmateGhost ||
                roleBehaviour is ICustomRole { Configuration.ShowInFreeplay: false })
            {
                continue;
            }

            var taskAddButton2 = Object.Instantiate(instance.RoleButton);
            taskAddButton2.SafePositionWorld = instance.SafePositionWorld;
            taskAddButton2.Text.text = "Be_" + roleBehaviour.NiceName + ".exe";
            taskAddButton2.Text.EnableMasking();

            instance.AddFileAsChildCustom(taskAddButton2, ref num, ref num2, ref num3);
            taskAddButton2.Role = roleBehaviour;
            if (roleBehaviour is ICustomRole custom)
            {
                var customColor = custom.IntroConfiguration?.IntroTeamColor ?? Color.gray;
                if (custom.Team is ModdedRoleTeams.Crewmate)
                {
                    customColor = Palette.CrewmateBlue;
                }
                else if (custom.Team is ModdedRoleTeams.Impostor)
                {
                    customColor = Palette.ImpostorRed;
                }
                taskAddButton2.FileImage.color = customColor;
                taskAddButton2.RolloverHandler.OutColor = customColor;
            }
            if (taskAddButton2.Button == null)
            {
                continue;
            }

            ControllerManager.Instance.AddSelectableUiElement(taskAddButton2.Button);
            switch (m)
            {
                case 0 when instance.restorePreviousSelectionFound != null:
                    ControllerManager.Instance.SetDefaultSelection(instance.restorePreviousSelectionFound);
                    instance.restorePreviousSelectionByFolderName = string.Empty;
                    instance.restorePreviousSelectionFound = null;
                    break;
                case 0:
                    ControllerManager.Instance.SetDefaultSelection(taskAddButton2.Button);
                    break;
            }
        }
    }
}
