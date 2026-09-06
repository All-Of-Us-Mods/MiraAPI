using System.Collections;
using System.Collections.Generic;
using AmongUs.GameOptions;
using MiraAPI.GameOptions;
using MiraAPI.PluginLoading;
using MiraAPI.Translation;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace MiraAPI.GameModes;

/// <summary>
/// Base class for custom gamemodes.
/// </summary>
[MiraIgnore]
public abstract class AbstractGameMode : IOptionable
{
    /// <summary>
    /// Gets the game mode name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the mode is visible in the menu. Only checked at startup.
    /// </summary>
    public virtual bool HideMode => false;

    /// <summary>
    /// Gets the game mode description.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the game mode color to display on the lobby pane.
    /// </summary>
    public virtual Color Color { get; } = Color.white;

    /// <summary>
    /// Gets the icon of the gamemode. Will be shown in options menu underneath the gamemode option.
    /// </summary>
    public virtual LoadableAsset<Sprite>? Icon => null;

    /// <summary>
    /// Gets or sets the icon of the gamemode for use on the options pop-up.
    /// </summary>
    public TMP_SpriteAsset TmpIcon { get; set; } = null!;

    /// <summary>
    /// Gets the colored game mode name, using the color and name properties.
    /// </summary>
    public string ColoredName => $"<color=#{Color.ToHtmlStringRGBA()}>{MiraLocaleManager.Get(Name)}</color>";

    /// <summary>
    /// Gets the game mode id.
    /// </summary>
    public uint ID { get; internal set; }

    /// <summary>
    /// Gets a value indicating whether a specific body type is used by the game mode.
    /// </summary>
    public virtual bool GameModeBodyTypeOverride => false;

    /// <summary>
    /// Gets a value indicating whether the gamemode removes the normal game settings.
    /// </summary>
    public virtual bool ShowNormalGameSettings => true;

    /// <summary>
    /// Gets a value indicating whether the gamemode removes the normal role settings.
    /// </summary>
    public virtual bool ShowNormalRoleSettings => true;

    /// <summary>
    /// Gets a value indicating the default kill cooldown for impostors in this mode.
    /// </summary>
    public virtual float DefaultImpostorKillCooldown => GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown);

    /// <summary>
    /// Called on GameManager.GetBodyType().
    /// </summary>
    /// <param name="player">Player to get body type from.</param>
    /// <returns>Resulting body type.</returns>
    public virtual PlayerBodyTypes GetBodyType(PlayerControl player)
    {
        return AprilFoolsMode.ShouldHorseAround()
            ? PlayerBodyTypes.Horse
            : (AprilFoolsMode.ShouldLongAround()
                ? PlayerBodyTypes.Long
                : PlayerBodyTypes.Normal);
    }

    /// <summary>
    /// Gets a value indicating whether a custom intro sequence is implemented by the game mode.
    /// </summary>
    public virtual bool ShowGameModeIntroCutscene => false;

    /// <summary>
    /// Called when Intro Cutscene is destroyed.
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Called on HudManager.Start().
    /// </summary>
    /// <param name="instance">Instance of HudManager.</param>
    public virtual void HudStart(HudManager instance)
    {
    }

    /// <summary>
    /// Called every frame on HudManager.Update().
    /// </summary>
    /// <param name="instance">Instance of HudManager.</param>
    public virtual void HudUpdate(HudManager instance)
    {
    }

    /// <summary>
    /// Called when a player is killed.
    /// </summary>
    /// <param name="player">PlayerControl that was killed.</param>
    /// <param name="assignGhostRole">Determines whether to assign a ghost role on the player.</param>
    public virtual void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
    {
        GameManager.Instance.LogicRoleSelection.OnPlayerDeath(player, assignGhostRole);
    }

    /// <summary>
    /// Calculate Game End logic.
    /// </summary>
    /// <param name="runOriginal">Should original game end logic be used.</param>
    /// <param name="instance">Instance of LogicGameFlowNormal.</param>
    public virtual void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
    {
        runOriginal = true;
    }

    /// <summary>
    /// Custom role assignment.
    /// </summary>
    /// <param name="runOriginal">Should original role assignment be used.</param>
    /// <param name="instance">Instance of LogicRoleSelection.</param>
    public virtual void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = true;
    }

    /// <summary>
    /// Custom role assignment to run after all roles are handed out.
    /// </summary>
    /// <param name="instance">Instance of LogicRoleSelection.</param>
    public virtual void PostAssignRoles(LogicRoleSelectionNormal instance)
    {
    }

    /// <summary>
    /// Custom murder logic.
    /// </summary>
    /// <param name="runOriginal">Should the original murder logic be used.</param>
    /// <param name="result">Result of custom murder logic.</param>
    /// <param name="target">Target player for murder.</param>
    public virtual void CanKill(out bool runOriginal, out bool result, PlayerControl target)
    {
        result = false;
        runOriginal = true;
    }

    /// <summary>
    /// Custom winner selection.
    /// </summary>
    /// <returns>List of winners or null.</returns>
    public virtual List<NetworkedPlayerInfo>? CalculateWinners()
    {
        return null;
    }

    /// <summary>
    /// The IEnumerator that plays the intro cutscene for this gamemode.
    /// </summary>
    /// <param name="introCutscene">An instance of IntroCutscene.</param>
    /// <returns>An IEnumerator to run the intro cutscene instead of the base game one.</returns>
    public virtual IEnumerator IntroCutscene(IntroCutscene introCutscene)
    {
        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// Can Admin be used in this gamemode.
    /// </summary>
    /// <param name="console">Admin Console.</param>
    /// <returns>True if Admin console is enabled.</returns>
    public virtual bool CanUseMapConsole(MapConsole console)
    {
        return true;
    }

    /// <summary>
    /// Can a body be reported in this gamemode.
    /// </summary>
    /// <param name="body">Target body for reporting.</param>
    /// <returns>True if dead bodies can be reported.</returns>
    public virtual bool CanReport(DeadBody body)
    {
        return true;
    }

    /// <summary>
    /// Can system consoles be used in this gamemode.
    /// </summary>
    /// <param name="console">System Console.</param>
    /// <returns>True if system consoles are enabled in this mode.</returns>
    public virtual bool CanUseSystemConsole(SystemConsole console)
    {
        return true;
    }

    /// <summary>
    /// Can tasks be interacted with in this gamemode.
    /// </summary>
    /// <param name="console">Task console.</param>
    /// <returns>True if tasks are enabled in this mode.</returns>
    public virtual bool CanUseTasks(Console console)
    {
        return true;
    }

    /// <summary>
    /// Should the sabotage map be used when attempting to open sabotage overlay.
    /// </summary>
    /// <param name="map">MapBehaviour object.</param>
    /// <returns>True if the sabotage map should be shown.</returns>
    public virtual bool ShouldShowSabotageMap(MapBehaviour map)
    {
        return true;
    }

    /// <summary>
    /// Gets the <see cref="MapOptions"/> to display on the map.
    /// </summary>
    /// <returns>The <see cref="MapOptions"/> for the gamemode.</returns>
    public virtual MapOptions GetMapOptions()
    {
        return new MapOptions
        {
            Mode = PlayerControl.LocalPlayer.Data.Role.IsImpostor && !MeetingHud.Instance
                ? MapOptions.Modes.Sabotage
                : MapOptions.Modes.Normal,
        };
    }

    /// <summary>
    /// Can a player vent in this gamemode.
    /// </summary>
    /// <param name="vent">Target vent.</param>
    /// <param name="playerInfo">Player attempting to vent.</param>
    /// <returns>True if venting is enabled in this mode.</returns>
    public virtual bool CanVent(Vent vent, NetworkedPlayerInfo playerInfo)
    {
        return true;
    }

    /// <summary>
    /// Gets a value indicating whether the task bar appears in the gamemode.
    /// </summary>
    /// <returns>True if the task bar is enabled in this mode.</returns>
    public virtual bool ShowTaskBar => true;

    /// <summary>
    /// Updates the task panel.
    /// </summary>
    /// <param name="instance">The task panel to update.</param>
    public virtual void UpdateTaskPanel(TaskPanelBehaviour instance)
    {
        instance.background.transform.localScale = (instance.taskText.textBounds.size.x > 0f)
            ? new Vector3(instance.taskText.textBounds.size.x + 0.2f, instance.taskText.textBounds.size.y + 0.2f, 1f)
            : Vector3.zero;
        Vector3 vector = instance.background.sprite.bounds.extents;
        vector.y = -vector.y;
        vector = vector.Mul(instance.background.transform.localScale);
        instance.background.transform.localPosition = vector;
        Vector3 vector2 = instance.tab.sprite.bounds.extents;
        vector2 = vector2.Mul(instance.tab.transform.localScale);
        vector2.y = -vector2.y;
        vector2.x += vector.x * 2f;
        instance.tab.transform.localPosition = vector2;
        if (GameManager.Instance == null)
        {
            return;
        }

        var yPos = 0.6f;
        var xPos = -instance.background.sprite.bounds.size.x * instance.background.transform.localScale.x;
        instance.closedPosition = new Vector3(xPos, yPos, instance.closedPosition.z);
        instance.openPosition = new Vector3(instance.openPosition.x, yPos, instance.openPosition.z);
        instance.timer = instance.open
            ? Mathf.Min(1f, instance.timer + Time.deltaTime / instance.animationTimeSeconds)
            : Mathf.Max(0f, instance.timer - Time.deltaTime / instance.animationTimeSeconds);

        Vector3 relativePos = new(
            Mathf.SmoothStep(instance.closedPosition.x, instance.openPosition.x, instance.timer),
            yPos,
            instance.openPosition.z);
        instance.transform.localPosition =
            AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop, relativePos);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name;
    }
}
