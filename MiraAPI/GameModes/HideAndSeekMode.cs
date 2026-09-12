using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using MiraAPI.GameOptions;
using MiraAPI.HnsReimplemented;
using MiraAPI.HnsReimplemented.Options;
using MiraAPI.PluginLoading;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using PowerTools;
using UnityEngine;

namespace MiraAPI.GameModes;

/// <summary>
/// The vanilla Hide and Seek game mode, ported to Mira.
/// </summary>
[MiraIgnore]
public class HideAndSeekMode : AbstractGameMode
{
    /// <inheritdoc/>
    public override string Name => "MiraApi.Gamemode.HideAndSeek";

    /// <inheritdoc/>
    public override string Description => "MiraApi.Gamemode.HideAndSeek.Description";

    /// <inheritdoc/>
    // TODO: Once the Hide n Seek port is ready for release, we will remove this.
    public override bool HideMode => !MiraApiPlugin.IsDevBuild;

    /// <inheritdoc/>
    public override Color Color { get; } = new Color32(255, 88, 90, 255);

    /// <inheritdoc/>
    public override LoadableAsset<Sprite> Icon => MiraAssets.HnSGamemodeIcon;

    /// <inheritdoc/>
    public override bool ShowGameModeIntroCutscene => true;

    /// <inheritdoc/>
    public override bool GameModeBodyTypeOverride => true;

    /// <inheritdoc/>
    public override bool ShowNormalGameSettings => false;

    /// <inheritdoc/>
    public override bool ShowNormalRoleSettings => false;

    /// <inheritdoc/>
    public override float DefaultImpostorKillCooldown => 1f;

    /// <inheritdoc/>
    public override bool ShowTaskBar => false;

    /// <inheritdoc/>
    public override bool CanReport(DeadBody body)
    {
        return false;
    }

    /// <inheritdoc/>
    public override bool ShouldShowSabotageMap(MapBehaviour map)
    {
        return false;
    }

    private static int ImpostorPlayerID()
    {
        return OptionGroupSingleton<HnsImpostorOptions>.Instance.SelectedSeeker.Value;
    }

    private static bool HasImpostorPlayerID()
    {
        return ImpostorPlayerID() > -1;
    }

    private static bool ValidateImpostorPlayerID(List<NetworkedPlayerInfo> players)
    {
        return HasImpostorPlayerID() && players.Find(p => p.PlayerId == ImpostorPlayerID()) != null;
    }

    /// <inheritdoc/>
    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;
        Il2CppSystem.Collections.Generic.List<ClientData> list = new();
        AmongUsClient.Instance.GetAllClients(list);
        List<NetworkedPlayerInfo> list2 = [.. list.ToArray()
            .Where(c => c.Character != null && c.Character.Data != null && !c.Character.Data.Disconnected &&
                        !c.Character.Data.IsDead).OrderBy(c => c.Id).Select(c => c.Character.Data)];

        foreach (var networkedPlayerInfo in GameData.Instance.AllPlayers)
        {
            if (networkedPlayerInfo.Object != null && networkedPlayerInfo.Object.isDummy)
            {
                list2.Add(networkedPlayerInfo);
            }
        }
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var adjustedNumImpostors = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(list2.Count);
        AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Impostor, Math.Max(adjustedNumImpostors, 1), RoleTypes.Impostor);
        AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Crewmate, int.MaxValue, RoleTypes.Engineer);
    }

    private static void AddGuaranteedRoles(IEnumerable<RoleBehaviour> guaranteedRoles, IGameOptions opts, List<RoleTypes> list)
    {
        var roleOptions = opts.RoleOptions;
        foreach (var roleAssignmentData in guaranteedRoles.Select(x =>
                     new RoleManager.RoleAssignmentData(x, roleOptions.GetNumPerGame(x.Role), 100)))
        {
            while (true)
            {
                var count = roleAssignmentData.Count;
                roleAssignmentData.Count = count - 1;
                if (count <= 0) break;

                list.Add(roleAssignmentData.Role.Role);
            }
        }
    }

    private static void AddPotentialRoles(IEnumerable<RoleBehaviour> source, IGameOptions opts, List<RoleTypes> list)
    {
        var roleOptions = opts.RoleOptions;
        var potentialRoles = source.Where(x => !x.IsDead).Select(role =>
            new RoleManager.RoleAssignmentData(
                role,
                roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();

        list.Clear();
        foreach (var roleData in potentialRoles)
        {
            for (var i = 0; i < roleData.Count; i++)
            {
                if (HashRandom.Next(101) < roleData.Chance)
                {
                    list.Add(roleData.Role.Role);
                }
            }
        }
    }

    private static void AddFallbackRoles(List<RoleTypes> list, int targetPlayerCount, int teamMax, int rolesAssigned, RoleTypes basicRole)
    {
        while (list.Count < targetPlayerCount && list.Count + rolesAssigned < teamMax)
        {
            list.Add(basicRole);
        }
    }

    private static void AssignRolesForTeam(
        List<NetworkedPlayerInfo> players,
        IGameOptions opts,
        RoleTeamTypes team,
        int teamMax,
        RoleTypes defaultRole)
    {
        Info($"Hide And Seek Mode - AssignRolesForTeam: Team: {team}, Max: {teamMax}, Players: {players.Count}, DefaultRole: {defaultRole}");
        var num = 0;
        var roleOptions = opts.RoleOptions;
        var source = RoleManager.Instance.AllRoles.ToArray()
            .Where(role => role.TeamType == team && !RoleManager.IsGhostRole(role.Role) &&
                           CustomRoleUtils.CanSpawnOnCurrentMode(role))
            .ToList();

        var assignmentData = source.Where(x => !x.IsDead).Select(role =>
            new RoleManager.RoleAssignmentData(
                role,
                roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();

        var source2 = CustomRoleUtils.GetPossibleRoles(assignmentData, x => x.Chance == 100);
        var guaranteedRoles = source.Where(x => source2.Contains(((ushort)x.Role, 100))).ToList();
        List<RoleTypes> list = [];

        switch (team)
        {
            case RoleTeamTypes.Crewmate:
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Before Guaranteed Assignment");
                AddGuaranteedRoles(guaranteedRoles, opts, list);
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Guaranteed Assignment");
                AssignRolesFromList(players, teamMax, list, ref num);

                AddPotentialRoles(source, opts, list);
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Potential Assignment");
                AssignRolesFromList(players, teamMax, list, ref num);

                const RoleTypes basicCrewRole = RoleTypes.Engineer;
                AddFallbackRoles(list, players.Count, teamMax, num, basicCrewRole);
                AssignRolesFromList(players, teamMax, list, ref num);
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Fallback Assignment");
                break;

            case RoleTeamTypes.Impostor:
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Before Guaranteed Assignment");
                var newImpostors = new List<NetworkedPlayerInfo>();

                // Specified Seeker
                if (HasImpostorPlayerID() &&
                    ValidateImpostorPlayerID(players) &&
                    !AmongUsClient.Instance.IsGamePublic)
                {
                    var networkedPlayerInfo = players.First(p => p.PlayerId == ImpostorPlayerID());
                    players.Remove(networkedPlayerInfo);
                    newImpostors.Add(networkedPlayerInfo);
                    Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Seeker is {networkedPlayerInfo.PlayerName}, ID: {networkedPlayerInfo.PlayerId}");
                }
                // Random Seeker
                else
                {
                    var num2 = 0;
                    while (num2 < teamMax && players.Count > 0)
                    {
                        PseudoRandomList<NetworkedPlayerInfo> pseudoRandomList = new(AmongUsClient.Instance.GameId);
                        players.Do(pseudoRandomList.Add);
                        for (var i = 0; i < GameData.RoundsPlayedInSession; i++)
                        {
                            pseudoRandomList.PickRandom();
                        }
                        var networkedPlayerInfo = pseudoRandomList.PickRandom();
                        players.Remove(networkedPlayerInfo);
                        newImpostors.Add(networkedPlayerInfo);
                        num2++;
                        Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Seeker is {networkedPlayerInfo.PlayerName}, ID: {networkedPlayerInfo.PlayerId}");
                    }
                }
                Info($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Guaranteed Assignment");

                AddGuaranteedRoles(guaranteedRoles, opts, list);
                AssignRolesFromList(newImpostors, teamMax, list, ref num);

                AddPotentialRoles(source, opts, list);
                AssignRolesFromList(newImpostors, teamMax, list, ref num);

                const RoleTypes basicImpRole = RoleTypes.Impostor;
                AddFallbackRoles(list, newImpostors.Count, teamMax, num, basicImpRole);
                AssignRolesFromList(newImpostors, teamMax, list, ref num);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(team), team, null);
        }
    }

    private static void AssignRolesFromList(List<NetworkedPlayerInfo> players, int teamMax, List<RoleTypes> roleList, ref int rolesAssigned)
    {
        while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
        {
            var index = HashRandom.FastNext(roleList.Count);
            var roleType = roleList[index];
            roleList.RemoveAt(index);
            var index2 = HashRandom.FastNext(players.Count);
            players[index2].Object.RpcSetRole(roleType);
            players.RemoveAt(index2);
            rolesAssigned++;
        }
    }

    /// <inheritdoc/>
    public override IEnumerator IntroCutscene(IntroCutscene introCutscene)
    {
        SoundManager.Instance.PlaySound(introCutscene.IntroStinger, false);
        Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Game Mode: Hide and Seek (MiraAPI)");
        introCutscene.LogPlayerRoleData();
        introCutscene.HideAndSeekPanels.SetActive(true);
        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            introCutscene.CrewmateRules.SetActive(false);
            introCutscene.ImpostorRules.SetActive(true);
        }
        else
        {
            introCutscene.CrewmateRules.SetActive(true);
            introCutscene.ImpostorRules.SetActive(false);
        }

        introCutscene.ImpostorName.gameObject.SetActive(true);
        introCutscene.ImpostorTitle.gameObject.SetActive(true);
        introCutscene.BackgroundBar.enabled = false;
        introCutscene.TeamTitle.gameObject.SetActive(false);
        var impostor = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.Role.IsImpostor);
        if (impostor == null)
        {
            Logger.GlobalInstance.Error("IntroCutscene :: CoBegin() :: impostor is NULL");
        }

        GameManager.Instance.SetSpecialCosmetics(impostor);
        introCutscene.ImpostorName.text = impostor != null ? impostor.Data.PlayerName : "???";

        yield return new WaitForSecondsRealtime(0.1f);
        if (impostor != null)
        {
            introCutscene.ImpostorTitle.text = impostor.Data.Role.GetRoleName();
        }
        PoolablePlayer? playerSlot = null;
        if (impostor != null)
        {
            playerSlot = introCutscene.CreatePlayer(1, 1, impostor.Data, false);
            playerSlot.SetBodyType(PlayerBodyTypes.Normal);
            playerSlot.SetFlipX(false);
            playerSlot.transform.localPosition = introCutscene.impostorPos;
            playerSlot.transform.localScale = Vector3.one * introCutscene.impostorScale;
        }

        yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
        yield return new WaitForSecondsRealtime(6f);
        if (playerSlot != null)
        {
            playerSlot.gameObject.SetActive(false);
        }

        introCutscene.HideAndSeekPanels.SetActive(false);
        introCutscene.CrewmateRules.SetActive(false);
        introCutscene.ImpostorRules.SetActive(false);
        HnsMusicHandler.Instance.StartMusicWithIntro();
        var hideTimer = 10f;

        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            introCutscene.HideAndSeekTimerText.gameObject.SetActive(true);
            PoolablePlayer poolablePlayer;
            AnimationClip anim;
            if (AprilFoolsMode.ShouldHorseAround())
            {
                poolablePlayer = introCutscene.HorseWrangleVisualSuit;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                anim = introCutscene.HnSSeekerSpawnHorseAnim;
                introCutscene.HorseWrangleVisualPlayer.SetBodyType(PlayerBodyTypes.Normal);
                introCutscene.HorseWrangleVisualPlayer.UpdateFromPlayerData(
                    PlayerControl.LocalPlayer.Data,
                    PlayerControl.LocalPlayer.CurrentOutfitType,
                    PlayerMaterial.MaskType.None,
                    false);
            }
            else if (AprilFoolsMode.ShouldLongAround())
            {
                poolablePlayer = introCutscene.HideAndSeekPlayerVisual;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.LongSeeker);
                anim = introCutscene.HnSSeekerSpawnLongAnim;
            }
            else
            {
                poolablePlayer = introCutscene.HideAndSeekPlayerVisual;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                anim = introCutscene.HnSSeekerSpawnAnim;
            }

            poolablePlayer.SetBodyCosmeticsVisible(false);
            poolablePlayer.UpdateFromPlayerData(
                PlayerControl.LocalPlayer.Data,
                PlayerControl.LocalPlayer.CurrentOutfitType,
                PlayerMaterial.MaskType.None,
                false);
            var component = poolablePlayer.GetComponent<SpriteAnim>();
            poolablePlayer.gameObject.SetActive(true);
            poolablePlayer.ToggleName(false);
            component.Play(anim);
            while (hideTimer > 0f)
            {
                introCutscene.HideAndSeekTimerText.text = Mathf.RoundToInt(hideTimer).ToString(CultureInfo.InvariantCulture);
                hideTimer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            HideAndSeekHudHelper.Instance.HideCountdown = hideTimer;
            if (AprilFoolsMode.ShouldHorseAround())
            {
                if (impostor != null)
                {
                    impostor.AnimateCustom(introCutscene.HnSSeekerSpawnHorseInGameAnim);
                }
            }
            else if (AprilFoolsMode.ShouldLongAround())
            {
                if (impostor != null)
                {
                    impostor.AnimateCustom(introCutscene.HnSSeekerSpawnLongInGameAnim);
                }
            }
            else if (impostor != null)
            {
                impostor.AnimateCustom(introCutscene.HnSSeekerSpawnAnim);
                impostor.cosmetics.SetBodyCosmeticsVisible(false);
            }
        }
        ShipStatus.Instance.StartSFX();
        HnsMusicHandler.Instance.OnGameStart();
        HnsDangerMeter.Instance.OnGameStart();
        UnityEngine.Object.Destroy(introCutscene.gameObject);
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        deadPlayerCount = 0;
        ShipStatus.Instance.BreakEmergencyButton();
        PlayerControl.LocalPlayer.SetKillTimer(0.01f);
        if (!HudManager.InstanceExists) return;
        HudManager.Instance.gameObject.AddComponent<HnsMusicHandler>();
        HudManager.Instance.gameObject.AddComponent<HideAndSeekHudHelper>();
        HudManager.Instance.gameObject.AddComponent<HnsDangerMeter>();
    }

    /// <inheritdoc/>
    public override MapOptions GetMapOptions()
    {
        MapOptions mapOptions = new()
        {
            Mode = MapOptions.Modes.Normal,
        };
        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor && HideAndSeekHudHelper.Instance.SeekerAdminMapEnabled(PlayerControl.LocalPlayer))
        {
            mapOptions.Mode = MapOptions.Modes.CountOverlay;
            mapOptions.AllowMovementWhileMapOpen = true;
            mapOptions.IncludeDeadBodies = false;
            mapOptions.ShowLivePlayerPosition = false;
        }
        return mapOptions;
    }

    /// <inheritdoc/>
    public override void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
    {
        runOriginal = false;
        var players = Helpers.GetAlivePlayers();
        if (!players.Any(x => x.Data.Role.IsImpostor))
        {
            instance.Manager.RpcEndGame(GameOverReason.ImpostorDisconnect, !DataManager.Player.Ads.HasPurchasedAdRemoval);
        }
        if (players.Any(x => !x.Data.Role.IsImpostor))
        {
            if (HideAndSeekHudHelper.Instance.AllTimersExpired())
            {
                instance.Manager.RpcEndGame(GameOverReason.HideAndSeek_CrewmatesByTimer, !DataManager.Player.Ads.HasPurchasedAdRemoval);
            }
            return;
        }
        instance.Manager.RpcEndGame(GameOverReason.HideAndSeek_ImpostorsByKills, !DataManager.Player.Ads.HasPurchasedAdRemoval);
    }

    /// <inheritdoc/>
    [SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Warning cascades into forcing the entire tree to be ternary operators.")]
    public override PlayerBodyTypes GetBodyType(PlayerControl player)
    {
        if (player == null || player.Data == null || player.Data.Role == null)
        {
            if (AprilFoolsMode.ShouldHorseAround())
            {
                return PlayerBodyTypes.Horse;
            }
            if (AprilFoolsMode.ShouldLongAround())
            {
                return PlayerBodyTypes.Long;
            }
            return PlayerBodyTypes.Normal;
        }

        if (AprilFoolsMode.ShouldHorseAround())
        {
            return player.Data.Role.IsImpostor ? PlayerBodyTypes.Normal : PlayerBodyTypes.Horse;
        }

        if (AprilFoolsMode.ShouldLongAround())
        {
            return player.Data.Role.IsImpostor ? PlayerBodyTypes.LongSeeker : PlayerBodyTypes.Long;
        }

        return player.Data.Role.IsImpostor ? PlayerBodyTypes.Seeker : PlayerBodyTypes.Normal;
    }

    /// <inheritdoc/>
    public override void UpdateTaskPanel(TaskPanelBehaviour instance)
    {
        instance.background.transform.localScale = (instance.taskText.textBounds.size.x > 0f)
            ? new Vector3(instance.taskText.textBounds.size.x + 0.2f, instance.taskText.textBounds.size.y + 0.2f, 1f)
            : Vector3.zero;
        var vector = instance.background.sprite.bounds.extents;
        vector.y = -vector.y;
        vector = vector.Mul(instance.background.transform.localScale);
        instance.background.transform.localPosition = vector;
        var vector2 = instance.tab.sprite.bounds.extents;
        vector2 = vector2.Mul(instance.tab.transform.localScale);
        vector2.y = -vector2.y;
        vector2.x += vector.x * 2f;
        instance.tab.transform.localPosition = vector2;
        if (GameManager.Instance == null)
        {
            return;
        }

        var yPos = 1.6f;
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

    private int deadPlayerCount;

    /// <inheritdoc/>
    public override void OnPlayerDeath(PlayerControl player, bool assignGhostRole)
    {
        base.OnPlayerDeath(player, assignGhostRole);
        HudManager.Instance.NotifyOfDeath();
        var popup = GameManagerCreator.Instance.HideAndSeekManagerPrefab.DeathPopupPrefab;
        deadPlayerCount++;
        var item = UnityEngine.Object.Instantiate(popup, HudManager.Instance.transform.parent);
        item.Show(player, deadPlayerCount);
    }
}
