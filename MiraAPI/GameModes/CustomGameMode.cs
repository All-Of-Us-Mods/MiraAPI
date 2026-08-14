using System.Collections.Generic;

namespace MiraAPI.GameModes;

/// <summary>
/// Base class for custom gamemodes.
/// </summary>
public abstract class CustomGameMode
{
    /// <summary>
    /// Gets the game mode name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the game mode description.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the game mode ID.
    /// </summary>
    public abstract int Id { get; }

    /// <summary>
    /// Called when <see cref="IntroCutscene"/> is destroyed.
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Called on <see cref="HudManager.Start"/>.
    /// </summary>
    /// <param name="instance">Instance of <see cref="HudManager"/>.</param>
    public virtual void HudStart(HudManager instance)
    {
    }

    /// <summary>
    /// Called every frame on <see cref="HudManager.Update"/>.
    /// </summary>
    /// <param name="instance">Instance of <see cref="HudManager"/>.</param>
    public virtual void HudUpdate(HudManager instance)
    {
    }

    /// <summary>
    /// Called when a player is killed.
    /// </summary>
    /// <param name="player"><see cref="PlayerControl"/> that was killed.</param>
    public virtual void OnDeath(PlayerControl player)
    {
    }

    /// <summary>
    /// Calculate Game End logic.
    /// </summary>
    /// <param name="runOriginal">Should original game end logic be used.</param>
    /// <param name="instance">Instance of <see cref="LogicGameFlowNormal"/>.</param>
    public virtual void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
    {
        runOriginal = true;
    }

    /// <summary>
    /// Custom role assignment.
    /// </summary>
    /// <param name="runOriginal">Should original role assignment be used.</param>
    /// <param name="instance">Instance of <see cref="LogicRoleSelection"/>.</param>
    public virtual void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = true;
    }

    /// <summary>
    /// Custom murder logic.
    /// </summary>
    /// <param name="runOriginal">Should the original murder logic be used.</param>
    /// <param name="result">Result of custom murder logic.</param>
    /// <param name="target">Target <see cref="PlayerControl"/> for murder.</param>
    public virtual void CanKill(out bool runOriginal, out bool result, PlayerControl target)
    {
        result = false;
        runOriginal = true;
    }

    /// <summary>
    /// Should Roles Settings be available when this gamemode is selected.
    /// </summary>
    /// <returns><see langword="true"/> if Role Settings are enabled in this game mode.</returns>
    public virtual bool AreRoleSettingsEnabled()
    {
        return true;
    }

    /// <summary>
    /// Should Game Settings be available when this gamemode is selected.
    /// </summary>
    /// <returns><see langword="true"/> if Game Settings are enabled in this mode.</returns>
    public virtual bool AreGameSettingsEnabled()
    {
        return true;
    }

    /// <summary>
    /// Custom winner selection.
    /// </summary>
    /// <returns>List of winners or <see langword="null"/>.</returns>
    public virtual List<NetworkedPlayerInfo>? CalculateWinners()
    {
        return null;
    }

    /// <summary>
    /// Show gamemode in <see cref="IntroCutscene"/>.
    /// </summary>
    /// <returns><see langword="true"/> if the game mode should be shown in the intro cutscene.</returns>
    public virtual bool ShowGameModeIntroCutscene()
    {
        return false;
    }

    /// <summary>
    /// Can Admin be used in this gamemode.
    /// </summary>
    /// <param name="console">Admin Console.</param>
    /// <returns><see langword="true"/> if Admin console is enabled.</returns>
    public virtual bool CanUseMapConsole(MapConsole console)
    {
        return true;
    }

    /// <summary>
    /// Can a <see cref="DeadBody"/> be reported in this gamemode.
    /// </summary>
    /// <param name="body">Target <see cref="DeadBody"/> for reporting.</param>
    /// <returns><see langword="true"/> if <see cref="DeadBody"/>s can be reported.</returns>
    public virtual bool CanReport(DeadBody body)
    {
        return true;
    }

    /// <summary>
    /// Can <see cref="SystemConsole"/>s be used in this gamemode.
    /// </summary>
    /// <param name="console">System Console.</param>
    /// <returns><see langword="true"/> if <see cref="SystemConsole"/>s are enabled in this mode.</returns>
    public virtual bool CanUseSystemConsole(SystemConsole console)
    {
        return true;
    }

    /// <summary>
    /// Can tasks be interacted with in this gamemode.
    /// </summary>
    /// <param name="console">Task console.</param>
    /// <returns><see langword="true"/> if tasks are enabled in this mode.</returns>
    public virtual bool CanUseTasks(Console console)
    {
        return true;
    }

    /// <summary>
    /// Should the sabotage map be used when attempting to open sabotage overlay.
    /// </summary>
    /// <param name="map"><see cref="MapBehaviour"/> object.</param>
    /// <returns><see langword="true"/> if the sabotage map should be shown.</returns>
    public virtual bool ShouldShowSabotageMap(MapBehaviour map)
    {
        return true;
    }

    /// <summary>
    /// Can a player vent in this gamemode.
    /// </summary>
    /// <param name="vent">Target vent.</param>
    /// <param name="playerInfo">Player attempting to vent.</param>
    /// <returns><see langword="true"/> if venting is enabled in this mode.</returns>
    public virtual bool CanVent(Vent vent, NetworkedPlayerInfo playerInfo)
    {
        return true;
    }
}
