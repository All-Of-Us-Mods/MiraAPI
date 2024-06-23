using AmongUs.GameOptions;
using Il2CppInterop.Runtime;
using MiraAPI.Networking;
using Reactor.Localization.Utilities;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Roles;

public static class CustomRoleManager
{
    public static Dictionary<ushort, RoleBehaviour> CustomRoles = new();

    public static int GetNextRoleId(ushort requestedId)
    {
        while (CustomRoles.ContainsKey(requestedId))
        {
            requestedId++;
        }

        return requestedId;
    }

    public static void RegisterInRoleManager()
    {
        RoleManager.Instance.AllRoles = RoleManager.Instance.AllRoles.Concat(CustomRoles.Values).ToArray();
    }

    internal static RoleBehaviour RegisterRole(Type roleType, ushort roleId)
    {
        if (!(typeof(RoleBehaviour).IsAssignableFrom(roleType) && typeof(ICustomRole).IsAssignableFrom(roleType)))
        {
            Logger<MiraAPIPlugin>.Error($"{roleType?.Name} does not inherit from RoleBehaviour or ICustomRole.");
            return null;
        }

        var roleBehaviour = (RoleBehaviour)new GameObject(roleType.Name).DontDestroy().AddComponent(Il2CppType.From(roleType));

        if (roleBehaviour is not ICustomRole customRole)
        {
            roleBehaviour.gameObject.Destroy();
            return null;
        }

        roleBehaviour.Role = (RoleTypes)GetNextRoleId(roleId);
        roleBehaviour.TeamType = customRole.Team == ModdedRoleTeams.Neutral ? RoleTeamTypes.Crewmate : (RoleTeamTypes)customRole.Team;
        roleBehaviour.NameColor = customRole.RoleColor;
        roleBehaviour.StringName = CustomStringName.CreateAndRegister(customRole.RoleName);
        roleBehaviour.BlurbName = CustomStringName.CreateAndRegister(customRole.RoleDescription);
        roleBehaviour.BlurbNameLong = CustomStringName.CreateAndRegister(customRole.RoleLongDescription);
        roleBehaviour.AffectedByLightAffectors = customRole.AffectedByLight;
        roleBehaviour.CanBeKilled = customRole.CanGetKilled;
        roleBehaviour.CanUseKillButton = customRole.CanKill;
        roleBehaviour.TasksCountTowardProgress = customRole.TasksCount;
        roleBehaviour.CanVent = customRole.CanUseVent;
        roleBehaviour.DefaultGhostRole = customRole.GhostRole;
        roleBehaviour.MaxCount = 15;

        if (customRole.IsGhostRole)
        {
            RoleManager.GhostRoles.Add(roleBehaviour.Role);
        }

        CustomRoles.Add(roleId, roleBehaviour);

        if (customRole.HideSettings)
        {
            return roleBehaviour;
        }

        var config = PluginSingleton<MiraAPIPlugin>.Instance.Config;
        config.Bind(customRole.NumConfigDefinition, 1);
        config.Bind(customRole.ChanceConfigDefinition, 100);

        return roleBehaviour;
    }


    public static bool GetCustomRoleBehaviour(RoleTypes roleType, out ICustomRole result)
    {
        CustomRoles.TryGetValue((ushort)roleType, out var temp);
        if (temp != null)
        {
            result = temp as ICustomRole;
            return true;
        }

        result = null;
        return false;
    }

    public static TaskPanelBehaviour CreateRoleTab(ICustomRole role)
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

    public static void UpdateRoleTab(TaskPanelBehaviour panel, ICustomRole role)
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

    public static void SyncRoleSettings()
    {
        foreach (var role in CustomRoles.Values)
        {
            ICustomRole customRole = role as ICustomRole;
            if (customRole.HideSettings)
            {
                continue;
            }

            PluginSingleton<MiraAPIPlugin>.Instance.Config.TryGetEntry<int>(customRole.NumConfigDefinition, out var numEntry);
            PluginSingleton<MiraAPIPlugin>.Instance.Config.TryGetEntry<int>(customRole.ChanceConfigDefinition, out var chanceEntry);

            Rpc<SyncRoleOptionsRpc>.Instance.Send(new SyncRoleOptionsRpc.Data((ushort)role.Role, numEntry.Value, chanceEntry.Value));
        }
    }

    public static void SyncRoleSettings(int targetId)
    {
        foreach (var role in CustomRoles.Values)
        {
            ICustomRole customRole = role as ICustomRole;
            if (customRole.HideSettings)
            {
                continue;
            }

            PluginSingleton<MiraAPIPlugin>.Instance.Config.TryGetEntry<int>(customRole.NumConfigDefinition, out var numEntry);
            PluginSingleton<MiraAPIPlugin>.Instance.Config.TryGetEntry<int>(customRole.ChanceConfigDefinition, out var chanceEntry);

            Rpc<SyncRoleOptionsRpc>.Instance.SendTo(targetId, new SyncRoleOptionsRpc.Data((ushort)role.Role, numEntry.Value, chanceEntry.Value));
        }
    }
}