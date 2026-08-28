using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Networking.Rpc;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Roles;

/// <summary>
/// Custom role manager for handling custom roles.
/// </summary>
public static class CustomRoleManager
{
    /// <summary>
    /// The default Among Us Crewmate Intro Sound.
    /// </summary>
    public static readonly LoadableAsset<AudioClip> CrewmateIntroSound =
        CustomRoleUtils.GetIntroSound(RoleTypes.Crewmate)!;

    /// <summary>
    /// The default Among Us Impostor Intro Sound.
    /// </summary>
    public static readonly LoadableAsset<AudioClip> ImpostorIntroSound =
        CustomRoleUtils.GetIntroSound(RoleTypes.Impostor)!;

    /// <summary>
    /// Gets the array of all roles from vanilla <see cref="RoleManager"/>.
    /// </summary>
    public static RoleBehaviour[] AllRoles => RoleManager.Instance.AllRoles.ToArray();

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{T}"/> of custom roles as <see cref="RoleBehaviour"/> objects.
    /// </summary>
    public static IReadOnlyList<RoleBehaviour> CustomRoleBehaviours { get; private set; } = [];

    /// <summary>
    /// Gets the <see cref="IReadOnlyList{T}"/> of custom roles as <see cref="ICustomRole"/> objects.
    /// </summary>
    public static IReadOnlyList<ICustomRole> CustomMiraRoles { get; private set; } = [];

    internal static readonly Dictionary<ushort, RoleBehaviour> CustomRoles = [];
    internal static readonly Dictionary<Type, ushort> RoleIds = [];

    private static Il2CppSystem.Collections.Generic.List<BaseGameSetting>? _emptySettings;
    private static Il2CppReferenceArray<OverlayKillAnimation>? _emptyKillAnimations;

    private static ushort _roleId = 100;

    private static ushort GetNextRoleId()
    {
        return _roleId++;
    }

    internal static void RegisterInRoleManager()
    {
        foreach (var role in CustomRoles.Values)
        {
            RoleManager.Instance.AllRoles.Add(role);
        }

        foreach (var role in CustomRoles.Values.Where(x => x.IsDead))
        {
            RoleManager.GhostRoles.Add(role.Role);
        }

        CustomRoleBehaviours = [.. CustomRoles.Values];
        CustomMiraRoles = [.. CustomRoles.Values.OfType<ICustomRole>()];
    }

    internal static void RegisterRoleTypes(List<Type> roles, MiraPluginInfo pluginInfo)
    {
        roles.ForEach(x => RoleIds.Add(x, GetNextRoleId()));

        var oldConfigSetting = pluginInfo.PluginConfig.SaveOnConfigSet;
        pluginInfo.PluginConfig.SaveOnConfigSet = false;

        foreach (var roleType in roles)
        {
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp(roleType);

                var role = RegisterRole(roleType, pluginInfo);
                if (role is null)
                {
                    continue;
                }

                pluginInfo.InternalRoles.Add((ushort)role.Role, role);
            }
            catch (Exception ex)
            {
                Error($"Failed to register role type {roleType.Name}: {ex}");
            }
        }

        pluginInfo.PluginConfig.Save();
        pluginInfo.PluginConfig.SaveOnConfigSet = oldConfigSetting;
    }

    private static RoleBehaviour? RegisterRole(Type roleType, MiraPluginInfo parentMod)
    {
        if (!(typeof(RoleBehaviour).IsAssignableFrom(roleType) && typeof(ICustomRole).IsAssignableFrom(roleType)))
        {
            Error($"{roleType.Name} does not inherit from RoleBehaviour or ICustomRole.");
            return null;
        }

        var roleBehaviour = (RoleBehaviour)new GameObject(roleType.Name).DontDestroy().AddComponent(Il2CppType.From(roleType));

        if (roleBehaviour is not ICustomRole customRole)
        {
            roleBehaviour.gameObject.Destroy();
            return null;
        }

        var roleId = RoleIds[roleType];

        roleBehaviour.Role = (RoleTypes)roleId;
        roleBehaviour.TeamType = customRole.Team == ModdedRoleTeams.Custom ? RoleTeamTypes.Crewmate : (RoleTeamTypes)customRole.Team;
        roleBehaviour.NameColor = customRole.RoleColor;
        roleBehaviour.StringName = MiraLocaleManager.GetOrCreateLocaleString(customRole.RoleNameLocale);
        roleBehaviour.BlurbName = MiraLocaleManager.GetOrCreateLocaleString(customRole.RoleDescriptionLocale);
        roleBehaviour.BlurbNameMed = MiraLocaleManager.GetOrCreateLocaleString(customRole.RoleMedDescriptionLocale);
        roleBehaviour.BlurbNameLong = MiraLocaleManager.GetOrCreateLocaleString(customRole.RoleLongDescriptionLocale);
        roleBehaviour.AffectedByLightAffectors = customRole.Configuration.AffectedByLightOnAirship;
        roleBehaviour.CanBeKilled = customRole.Configuration.CanGetKilled;
        roleBehaviour.CanUseKillButton = customRole.Configuration.UseVanillaKillButton;
        roleBehaviour.TasksCountTowardProgress = customRole.Configuration.TasksCountForProgress;
        roleBehaviour.CanVent = customRole.Configuration.CanUseVent || customRole.Configuration.GetsVentData;
        roleBehaviour.DefaultGhostRole = customRole.Configuration.GhostRole;
        roleBehaviour.MaxCount = customRole.Configuration.MaxRoleCount;
        roleBehaviour.RoleScreenshot = customRole.Configuration.OptionsScreenshot != null
            ? customRole.Configuration.OptionsScreenshot.LoadAsset()
            : Sprite.Create(
                null,
                new Rect(0, 0, 370, 230),
                Vector2.one / 2,
                100);

        _emptySettings ??= new(0);
        _emptyKillAnimations ??= new(0);

        roleBehaviour.AllGameSettings = _emptySettings;
        roleBehaviour.CustomKillAnimations = _emptyKillAnimations;

        if (customRole.Configuration.Icon != null)
        {
            var asset = customRole.Configuration.Icon.LoadAsset();
            if (asset != null)
            {
                roleBehaviour.RoleIconSolid = asset;
                roleBehaviour.RoleIconWhite = asset;
            }
        }

        if (customRole.Configuration.IntroSound != null)
        {
            roleBehaviour.IntroSound = customRole.Configuration.IntroSound.LoadAsset();
        }

        if (roleBehaviour.IsDead)
        {
            RoleManager.GhostRoles.Add(roleBehaviour.Role);
        }

        var useTaskHint = customRole.Configuration.RoleHintType is RoleHintType.TaskHint;
        var overridesTaskText = customRole.GetType().GetMethod("SpawnTaskHeader")?.DeclaringType == customRole.GetType();

        if (useTaskHint && !overridesTaskText)
        {
            Error($"Role {customRole.RoleName} is using RoleHintType.TaskHint but does not override SpawnTaskHeader!");
        }

        CustomRoles.Add(roleId, roleBehaviour);

        if (customRole.Configuration.HideSettings)
        {
            return roleBehaviour;
        }

        var config = parentMod.PluginConfig;
        config.Bind(customRole.NumConfigDefinition, Mathf.Clamp(customRole.Configuration.DefaultRoleCount, 0, customRole.Configuration.MaxRoleCount));
        config.Bind(customRole.ChanceConfigDefinition, Mathf.Clamp(customRole.Configuration.DefaultChance, 0, 100));

        return roleBehaviour;
    }

    /// <summary>
    /// Finds the parent mod of a custom role.
    /// </summary>
    /// <param name="role">The <see cref="ICustomRole"/> object.</param>
    /// <returns>A <see cref="MiraPluginInfo"/> object representing the parent mod of the role.</returns>
    public static MiraPluginInfo FindParentMod(ICustomRole role)
    {
        return MiraPluginManager.Instance.RegisteredPlugins.First(plugin => plugin.InternalRoles.ContainsValue(role as RoleBehaviour ?? throw new InvalidOperationException()));
    }

    /// <summary>
    /// Gets a custom role behaviour by <see cref="RoleTypes"/>.
    /// </summary>
    /// <param name="roleType">The <see cref="RoleTypes"/> <see langword="enum"/>.</param>
    /// <param name="result">The <see cref="ICustomRole"/> result.</param>
    /// <returns><see langword="true"/> if the role was found.</returns>
    public static bool GetCustomRoleBehaviour(RoleTypes roleType, out ICustomRole? result)
    {
        CustomRoles.TryGetValue((ushort)roleType, out var temp);
        if (temp)
        {
            result = temp as ICustomRole;
            return true;
        }

        result = null;
        return false;
    }

    internal static TaskPanelBehaviour CreateRoleTab(ICustomRole role)
    {
        var ogPanel = HudManager.Instance.TaskStuff.transform.FindChild("TaskPanel").gameObject.GetComponent<TaskPanelBehaviour>();
        var clonePanel = Object.Instantiate(ogPanel.gameObject, ogPanel.transform.parent);
        clonePanel.name = "RolePanel";

        var newPanel = clonePanel.GetComponent<TaskPanelBehaviour>();
        newPanel.open = false;

        var tab = newPanel.tab.gameObject;
        tab.GetComponentInChildren<TextTranslatorTMP>().Destroy();

        newPanel.transform.localPosition = ogPanel.transform.localPosition - new Vector3(0, 1, 0);

        UpdateRoleTab(newPanel, role);
        return newPanel;
    }

    internal static void UpdateRoleTab(TaskPanelBehaviour panel, ICustomRole role)
    {
        var tabText = panel.tab.gameObject.GetComponentInChildren<TextMeshPro>();
        var ogPanel = HudManager.Instance.TaskStuff.transform.FindChild("TaskPanel").gameObject.GetComponent<TaskPanelBehaviour>();
        if (tabText.text != role.RoleName)
        {
            tabText.text = role.RoleName;
        }

        var y = ogPanel.taskText.textBounds.size.y + 1;
        panel.closedPosition = new Vector3(ogPanel.closedPosition.x, ogPanel.open ? y + 0.2f : 2f, ogPanel.closedPosition.z);
        panel.openPosition = new Vector3(ogPanel.openPosition.x, ogPanel.open ? y : 2f, ogPanel.openPosition.z);

        panel.SetTaskText(role.SetTabText().ToString());
    }

    internal static void SyncAllRoleSettings(int targetId = -1)
    {
        var data = CustomRoles.Values
            .Where(x => x is ICustomRole { Configuration.HideSettings: false })
            .Select(x => ((ICustomRole)x).GetNetData())
            .ChunkNetData(1000);

        while (data.Count > 0)
        {
            Rpc<SyncRoleOptionsRpc>.Instance.SendTo(PlayerControl.LocalPlayer, targetId, data.Dequeue());
        }
    }

    internal static void HandleSyncRoleOptions(NetData[] data)
    {
        // necessary to disable then re-enable this setting
        // we dont know how other plugins handle their configs
        // this way, all the options are saved at once, instead of one by one
        var oldConfigSetting = new Dictionary<MiraPluginInfo, bool>();
        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
        {
            oldConfigSetting.Add(plugin, plugin.PluginConfig.SaveOnConfigSet);
            plugin.PluginConfig.SaveOnConfigSet = false;
        }

        foreach (var netData in data)
        {
            if (!CustomRoles.TryGetValue((ushort)netData.Id, out var role))
            {
                continue;
            }

            var customRole = role as ICustomRole;
            if (customRole is null or { Configuration.HideSettings: true })
            {
                continue;
            }

            var num = BitConverter.ToInt32(netData.Data, 0);
            var chance = BitConverter.ToInt32(netData.Data, 4);

            HudManager.Instance.Notifier.AddRoleSettingsChangeMessage(role.StringName, num, chance, role.TeamType, false);

            try
            {
                customRole.SetCount(num);
                customRole.SetChance(chance);
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
        {
            plugin.PluginConfig.Save();
            plugin.PluginConfig.SaveOnConfigSet = oldConfigSetting[plugin];
        }

        if (LobbyInfoPane.Instance)
        {
            LobbyInfoPane.Instance.RefreshPane();
        }
    }
}
