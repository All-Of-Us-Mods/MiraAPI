using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Modifiers;
using MiraAPI.Modifiers.Types;
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

namespace MiraAPI.Patches.Freeplay;

[HarmonyPatch(typeof(TaskAdderGame))]
public static class TaskAdderPatches
{
    private static Scroller _scroller = null!;
    private static readonly Dictionary<string, TaskFolder> Folders = [];

    public static string CrewmateName => TranslationController.Instance.GetString(StringNames.Crewmate);
    public static string ImpostorName => TranslationController.Instance.GetString(StringNames.Impostor);
    public static string NeutralName => "Neutral";
    public static string ModifiersName => "Modifiers";

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
        var crewmateFolder = __instance.Root.SubFolders.ToArray().FirstOrDefault(x => x.FolderName == CrewmateName)!;
        var impostorFolder = __instance.Root.SubFolders.ToArray().FirstOrDefault(x => x.FolderName == ImpostorName)!;
        // var neutralFolder = __instance.CreateFolder("Neutral", __instance.Root, 2, Color.gray);
        var modifiersFolder = __instance.CreateFolder(ModifiersName, __instance.Root, 0, Color.blue);

        Folders.Clear();
        Folders.Add(crewmateFolder.FolderName, crewmateFolder);
        Folders.Add(impostorFolder.FolderName, impostorFolder);
        // folders.Add("Neutrals", neutralFolder);
        Folders.Add(ModifiersName, modifiersFolder);

        var folderIdx = 2;
        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
        {
            var pluginFolders = new Dictionary<string, TaskFolder>();

            foreach (var role in plugin.InternalRoles
                         .Where(x => x.Value is ICustomRole { Configuration.ShowInFreeplay: true })
                         .Select(x => x.Value))
            {
                var customRole = role as ICustomRole;
                var folderName = customRole!.Configuration.FreeplayFolder;
                if (!Folders.TryGetValue(folderName, out var teamFolder))
                {
                    teamFolder = __instance.CreateFolder(folderName, __instance.Root, folderIdx++, customRole.IntroConfiguration?.IntroTeamColor ?? Color.gray);
                    Folders.Add(folderName, teamFolder);
                }

                if (!pluginFolders.TryGetValue(folderName, out var pluginFolder))
                {
                    pluginFolder = __instance.CreateFolder(plugin.PluginInfo.Metadata.Name, teamFolder);
                    pluginFolder.name = $"Roles:{folderName}:{plugin.PluginId}";
                    pluginFolders.Add(folderName, teamFolder);
                }
            }

            if (plugin.InternalModifiers.Exists(x => x.ShowInFreeplay))
            {
                __instance.CreateFolder(plugin.PluginInfo.Metadata.Name, modifiersFolder)
                    .gameObject.name = plugin.PluginId;
            }
        }

        // Modifier folder at the end
        __instance.Root.SubFolders.Insert(folderIdx, modifiersFolder);

        __instance.GoToRoot();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TaskAdderGame.PopulateRoot))]
    private static bool PopulateRootPrefix(TaskAdderGame __instance, TaskAdderGame.FolderType folderType, TaskFolder rootFolder, Il2CppSystem.Collections.Generic.Dictionary<string, TaskFolder> folders, Il2CppReferenceArray<NormalPlayerTask> taskList)
    {
        if (folderType != TaskAdderGame.FolderType.Tasks)
        {
            if (folderType != TaskAdderGame.FolderType.Roles)
            {
                return false;
            }

            var crewFolder = folders["Crewmates"] =
                Object.Instantiate(__instance.RootFolderPrefab, __instance.transform);
            crewFolder.gameObject.SetActive(false);
            crewFolder.SetFolderColor(TaskFolder.FolderColor.Blue);
            var impFolder = folders["Impostors"] =
                Object.Instantiate(__instance.RootFolderPrefab, __instance.transform);
            impFolder.gameObject.SetActive(false);
            impFolder.SetFolderColor(TaskFolder.FolderColor.Red);
            crewFolder.FolderName = CrewmateName;
            impFolder.FolderName = ImpostorName;
            rootFolder.SubFolders.Insert(0, crewFolder);
            rootFolder.SubFolders.Insert(0, impFolder);
            Il2CppSystem.Collections.Generic.List<RoleBehaviour> impRoles = new();
            Il2CppSystem.Collections.Generic.List<RoleBehaviour> crewRoles = new();
            foreach (var role in RoleManager.Instance.AllRoles)
            {
                if (role.Role != RoleTypes.ImpostorGhost && role.Role != RoleTypes.CrewmateGhost && !role.IsCustomRole())
                {
                    if (role.TeamType == RoleTeamTypes.Crewmate)
                    {
                        crewRoles.Add(role);
                    }
                    else
                    {
                        impRoles.Add(role);
                    }
                }
            }

            crewFolder.RoleChildren = crewRoles;
            impFolder.RoleChildren = impRoles;
            return false;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract (Justification: Nullable annotations of reference types do not survive compilation.)
        if (taskList == null)
        {
            return false;
        }

        foreach (var normalPlayerTask in taskList)
        {
            var systemTypes = normalPlayerTask.StartAt;
            if (normalPlayerTask is DivertPowerTask task)
            {
                systemTypes = task.TargetSystem;
            }

            if (systemTypes == SystemTypes.LowerEngine)
            {
                systemTypes = SystemTypes.UpperEngine;
            }

            if (!folders.TryGetValue(systemTypes.ToString(), out var taskFolder3))
            {
                taskFolder3 = folders[systemTypes.ToString()] = Object.Instantiate(__instance.RootFolderPrefab, __instance.transform);
                taskFolder3.SetFolderColor(TaskFolder.FolderColor.Tan);
                taskFolder3.gameObject.SetActive(false);
                taskFolder3.FolderName = systemTypes == SystemTypes.UpperEngine
                    ? TranslationController.Instance.GetString(StringNames.Engines)
                    : TranslationController.Instance.GetString(systemTypes);

                rootFolder.SubFolders.Add(taskFolder3);
            }

            taskFolder3.TaskChildren.Add(normalPlayerTask);
        }

        return false;
    }

    private static TaskFolder CreateFolder(this TaskAdderGame instance, string name, TaskFolder parent, int idx = -1, Color? color = null)
    {
        var folder = Object.Instantiate(instance.RootFolderPrefab, instance.transform);
        folder.gameObject.SetActive(false);
        folder.FolderName = name;
        folder.SetCustomColor(color ?? new Color(0.937f, 0.811f, 0.592f));
        folder.name = name + "Folder";
        if (idx == -1)
            parent.SubFolders.Add(folder);
        else if (idx > 0)
            parent.SubFolders.Insert(idx, folder);
        return folder;
    }

    private static void SetCustomColor(this TaskFolder folder, Color color)
    {
        folder.currentFolderColor = color;
        folder.folderSpriteRenderer.color = color;
        folder.buttonRolloverHandler.OutColor = color;
        folder.buttonRolloverHandler.UnselectedColor = color;
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

    private static bool IsChildOf(this TaskFolder child, TaskFolder parent)
    {
        return parent.SubFolders
            .ToArray()
            .Any(x => x.FolderName == child.FolderName);
    }

    // yes it might be crazy patching the entire method, but I tried so many other methods and only this works :cry:
    // true -chip
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TaskAdderGame.ShowFolder))]
    public static bool ShowPatch(TaskAdderGame __instance, TaskFolder taskFolder)
    {
        var stringBuilder = new StringBuilder(64);
        __instance.Hierarchy.Add(taskFolder);
        foreach (var t in __instance.Hierarchy)
        {
            stringBuilder.Append(t.FolderName);
            stringBuilder.Append('\\');
        }
        __instance.PathText.text = stringBuilder.ToString();
        __instance.PathText.fontSizeMin = 3;
        __instance.PathText.fontSizeMax = 3;
        __instance.transform.FindChild("TitleText_TMP")?.gameObject.DestroyImmediate();

        __instance.ActiveItems.ToArray().Do(x => x.gameObject.DeepDestroy(false));
        __instance.ActiveItems.Clear();

        var num = 0f;
        var num2 = 0f;
        var num3 = 0f;
        foreach (var t in taskFolder.SubFolders)
        {
            var taskFolder2 = Object.Instantiate(t, __instance.TaskParent);
            taskFolder2.gameObject.SetActive(true);
            taskFolder2.Parent = __instance;
            taskFolder2.transform.localPosition = new Vector3(num, num2, 0f);
            taskFolder2.transform.localScale = Vector3.one;
            num3 = Mathf.Max(num3, taskFolder2.Text.bounds.size.y + 1.1f);
            num += __instance.folderWidth;
            if (num > __instance.lineWidth)
            {
                num = 0f;
                num2 -= num3;
                num3 = 0f;
            }

            __instance.ActiveItems.Add(taskFolder2.transform);
            if (taskFolder2 != null && taskFolder2.Button != null)
            {
                ControllerManager.Instance.AddSelectableUiElement(taskFolder2.Button);
            }
        }

        var list = taskFolder.TaskChildren
            .ToArray()
            .OrderBy(x => x.TaskType)
            .ToArray();
        foreach (var task in list)
        {
            var taskAddButton = Object.Instantiate(__instance.TaskPrefab);
            taskAddButton.MyTask = task;
            switch (task.TaskType)
            {
                case TaskTypes.DivertPower:
                    var targetSystem = task.Cast<DivertPowerTask>().TargetSystem;
                    taskAddButton.Text.text = TranslationController.Instance.GetString(
                        StringNames.DivertPowerTo,
                        TranslationController.Instance.GetString(targetSystem));
                    break;
                case TaskTypes.FixWeatherNode:
                    var nodeId = task.Cast<WeatherNodeTask>().NodeId;
                    taskAddButton.Text.text =
                        TranslationController.Instance.GetString(
                            StringNames.FixWeatherNode) + " " +
                        TranslationController.Instance.GetString(
                            WeatherSwitchGame.ControlNames[nodeId]);
                    break;
                default:
                    taskAddButton.Text.text =
                        TranslationController.Instance.GetString(task.TaskType);
                    break;
            }

            __instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);
            if (taskAddButton != null && taskAddButton.Button != null)
            {
                ControllerManager.Instance.AddSelectableUiElement(taskAddButton.Button);
            }
        }

        var roleChildren = taskFolder.RoleChildren
            .ToArray()
            .Where(x => x is not ICustomRole)
            .ToArray();
        foreach (var role in roleChildren)
        {
            var roleAddButton = Object.Instantiate(__instance.RoleButton);
            roleAddButton.SafePositionWorld = __instance.SafePositionWorld;
            roleAddButton.Text.text = role.GetRoleName();
            roleAddButton.Role = role;
            roleAddButton.Text.fontSizeMin = 1;
            roleAddButton.FileImage.sprite = role.IsImpostor
                ? MiraAssets.ImpostorFile.LoadAsset()
                : MiraAssets.CrewmateFile.LoadAsset();

            __instance.AddFileAsChildCustom(roleAddButton, ref num, ref num2, ref num3);
            if (roleAddButton != null && roleAddButton.Button != null)
            {
                ControllerManager.Instance.AddSelectableUiElement(roleAddButton.Button);
            }
        }

        string[] split = [];
        try
        {
            split = taskFolder.name.Replace("(Clone)", string.Empty).Split(':');
        }
        catch
        {
            // I hate you
        }
        if (split is ["Roles", _, _] && Folders.Any(x => taskFolder.IsChildOf(x.Value)))
        {
            var plugin = MiraPluginManager.GetPluginByGuid(split[2]);
            if (plugin != null)
            {
                foreach (var role in plugin.InternalRoles)
                {
                    var roleBehaviour = role.Value;
                    if (roleBehaviour.Role == RoleTypes.ImpostorGhost || roleBehaviour.Role == RoleTypes.CrewmateGhost ||
                        roleBehaviour is ICustomRole { Configuration.ShowInFreeplay: false })
                    {
                        continue;
                    }
                    var crewLocale = TranslationController.Instance.GetString(StringNames.Crewmate);
                    var impLocale = TranslationController.Instance.GetString(StringNames.Impostor);

                    var teamFolder = roleBehaviour.IsImpostor ? impLocale : crewLocale;
                    if (roleBehaviour is ICustomRole cs)
                    {
                        teamFolder = cs.Configuration.FreeplayFolder;
                    }
                    if (split[1] != teamFolder)
                    {
                        continue;
                    }

                    var roleAddButton = Object.Instantiate(__instance.RoleButton);
                    roleAddButton.MyTask = null;
                    roleAddButton.SafePositionWorld = __instance.SafePositionWorld;
                    roleAddButton.Text.text = roleBehaviour.GetRoleName();
                    roleAddButton.Text.EnableMasking();
                    roleAddButton.role = roleBehaviour;
                    roleAddButton.Text.fontSizeMin = 1;
                    roleAddButton.FileImage.sprite = roleBehaviour.IsImpostor
                        ? MiraAssets.ImpostorFile.LoadAsset()
                        : MiraAssets.CrewmateFile.LoadAsset();
                    if (roleBehaviour is ICustomRole custom)
                    {
                        if (custom.Team == ModdedRoleTeams.Custom) roleAddButton.FileImage.sprite = MiraAssets.CustomTeamFile.LoadAsset();
                        var customColor = custom.IntroConfiguration?.IntroTeamColor ?? Color.gray;
                        if (custom.Team is ModdedRoleTeams.Crewmate)
                        {
                            customColor = Palette.CrewmateBlue;
                        }
                        else if (custom.Team is ModdedRoleTeams.Impostor)
                        {
                            customColor = Palette.ImpostorRed;
                        }
                        roleAddButton.FileImage.color = customColor;
                        roleAddButton.RolloverHandler.OutColor = customColor;
                    }

                    __instance.AddFileAsChildCustom(roleAddButton, ref num, ref num2, ref num3);
                    ControllerManager.Instance.AddSelectableUiElement(roleAddButton.Button);
                }
            }
        }

        if (Folders.TryGetValue(ModifiersName, out var modifiersFolder) && taskFolder.IsChildOf(modifiersFolder))
        {
            var plugin = MiraPluginManager.GetPluginByGuid(taskFolder.name.Replace("(Clone)", string.Empty));
            if (plugin != null)
            {
                foreach (var modifier in plugin.InternalModifiers)
                {
                    if (!modifier.ShowInFreeplay)
                    {
                        continue;
                    }

                    var taskAddButton = Object.Instantiate(__instance.RoleButton);
                    taskAddButton.name = modifier.TypeId.ToString(NumberFormatInfo.InvariantInfo);
                    taskAddButton.role = null;
                    taskAddButton.MyTask = null;
                    taskAddButton.SafePositionWorld = __instance.SafePositionWorld;
                    taskAddButton.Text.text = modifier.ModifierName.Translate();
                    taskAddButton.Text.fontSizeMin = 1;
                    taskAddButton.Text.EnableMasking();
                    taskAddButton.FileImage.color = modifier.FreeplayFileColor;
                    taskAddButton.RolloverHandler.OutColor = modifier.FreeplayFileColor;
                    if (modifier is TimedModifier timed)
                    {
                        taskAddButton.FileImage.sprite = MiraAssets.TimedModifierFile.LoadAsset();
                        taskAddButton.Text.text += $" ({timed.Duration}s)";
                    }
                    else
                    {
                        taskAddButton.FileImage.sprite = MiraAssets.ModifierFile.LoadAsset();
                    }

                    taskAddButton.Button.OnClick = new Button.ButtonClickedEvent();
                    var m = modifier;
                    taskAddButton.Button.OnClick.AddListener((UnityAction)(() =>
                    {
                        var id = m.TypeId;
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

                    __instance.AddFileAsChildCustom(taskAddButton, ref num, ref num2, ref num3);
                    ControllerManager.Instance.AddSelectableUiElement(taskAddButton.Button);
                }
            }
        }

        if (_scroller)
        {
            _scroller.CalculateAndSetYBounds(__instance.ActiveItems.Count, 6, 4.5f, 1.65f);
            _scroller.SetYBoundsMin(0.0f);
            _scroller.ScrollToTop();
        }

        ControllerManager.Instance.SetBackButton(__instance.Hierarchy.Count == 1
            ? __instance.BackButton
            : __instance.FolderBackButton);

        __instance.SetPreviousControllerSelection(taskFolder.FolderName);
        return false;
    }
}
