using System;
using System.Collections;
using System.Collections.Generic;
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

    // TODO: Once the Hide n Seek port is ready for release, we will remove this.
    public override bool HideMode => !MiraApiPlugin.IsDevBuild;

    public override Color Color { get; } = new Color32(255, 88, 90, 255);
    public override LoadableAsset<Sprite>? Icon => MiraAssets.HnSGamemodeIcon;
    public override bool CanReport(DeadBody body)
    {
        return false;
    }

    public override bool ShouldShowSabotageMap(MapBehaviour map)
    {
        return false;
    }

    public override bool ShowGameModeIntroCutscene => true;
    public override bool GameModeBodyTypeOverride => true;
    public override bool ShowNormalGameSettings => false;
    public override bool ShowNormalRoleSettings => false;
    public override float DefaultImpostorKillCooldown => 1f;

    public override bool ShowTaskBar => false;
    public static int ImpostorPlayerID()
    {
        return OptionGroupSingleton<HnsImpostorOptions>.Instance.SelectedSeeker.Value;
    }

    public static bool HasImpostorPlayerID()
    {
        return ImpostorPlayerID() > -1;
    }

    public static bool ValidateImpostorPlayerID(List<NetworkedPlayerInfo> players)
    {
        return HasImpostorPlayerID() && players.Find((NetworkedPlayerInfo p) => (int)p.PlayerId == ImpostorPlayerID()) != null;
    }

    public override void AssignRoles(out bool runOriginal, LogicRoleSelectionNormal instance)
    {
        runOriginal = false;
        Il2CppSystem.Collections.Generic.List<ClientData> list = new();
        AmongUsClient.Instance.GetAllClients(list);
        List<NetworkedPlayerInfo> list2 = list.ToArray()
            .Where(c => c.Character != null && c.Character.Data != null && !c.Character.Data.Disconnected &&
                        !c.Character.Data.IsDead).OrderBy(c => c.Id).Select(c => c.Character.Data)
            .ToList();

        foreach (NetworkedPlayerInfo networkedPlayerInfo in GameData.Instance.AllPlayers)
        {
            if (networkedPlayerInfo.Object != null && networkedPlayerInfo.Object.isDummy)
            {
                list2.Add(networkedPlayerInfo);
            }
        }
        IGameOptions currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        int adjustedNumImpostors = GameOptionsManager.Instance.CurrentGameOptions.GetAdjustedNumImpostors(list2.Count);
        AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Impostor, Math.Max(adjustedNumImpostors, 1), RoleTypes.Impostor);
        AssignRolesForTeam(list2, currentGameOptions, RoleTeamTypes.Crewmate, int.MaxValue, RoleTypes.Engineer);
    }

    public static void AssignRolesForTeam(
        List<NetworkedPlayerInfo> players,
        IGameOptions opts,
        RoleTeamTypes team,
        int teamMax,
        RoleTypes defaultRole)
    {
        Error($"Hide And Seek Mode - AssignRolesForTeam: Team: {team}, Max: {teamMax}, Players: {players.Count}, DefaultRole: {defaultRole}");
        int num = 0;
        IRoleOptionsCollection roleOptions = opts.RoleOptions;
        var source = RoleManager.Instance.AllRoles.ToArray()
            .Where(role => role.TeamType == team && !RoleManager.IsGhostRole(role.Role) &&
                           CustomRoleUtils.CanSpawnOnCurrentMode(role));
        var assignmentData = source.Where(x => !x.IsDead).Select(role =>
            new RoleManager.RoleAssignmentData(
                role,
                roleOptions.GetNumPerGame(role.Role),
                roleOptions.GetChancePerGame(role.Role))).ToList();
        var source2 = CustomRoleUtils.GetPossibleRoles(assignmentData, x => x.Chance == 100);
        var guaranteedRoles = source.Where(x => source2.Contains(((ushort)x.Role, 100)));
        List<RoleTypes> list = new List<RoleTypes>();
        if (team == RoleTeamTypes.Crewmate)
        {
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Before Guaranteed Assignment");
            foreach (RoleManager.RoleAssignmentData roleAssignmentData in guaranteedRoles.Select((x) =>
                         new RoleManager.RoleAssignmentData(x, roleOptions.GetNumPerGame(x.Role), 100)))
            {
                while (true)
                {
                    RoleManager.RoleAssignmentData roleAssignmentData2 = roleAssignmentData;
                    int count = roleAssignmentData2.Count;
                    roleAssignmentData2.Count = count - 1;
                    if (count <= 0)
                    {
                        break;
                    }

                    list.Add(roleAssignmentData.Role.Role);
                }
            }
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Guaranteed Assignment");
            AssignRolesFromList(players, teamMax, list, ref num);

            var list2 = source.Where(x => !x.IsDead).Select(role =>
                new RoleManager.RoleAssignmentData(
                    role,
                    roleOptions.GetNumPerGame(role.Role),
                    roleOptions.GetChancePerGame(role.Role))).ToList();

            list.Clear();
            foreach (RoleManager.RoleAssignmentData roleAssignmentData3 in list2)
            {
                for (int i = 0; i < roleAssignmentData3.Count; i++)
                {
                    if (HashRandom.Next(101) < roleAssignmentData3.Chance)
                    {
                        list.Add(roleAssignmentData3.Role.Role);
                    }
                }
            }
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Potential Assignment");

            AssignRolesFromList(players, teamMax, list, ref num);
            var basicRole = RoleTypes.Engineer;
            while (list.Count < players.Count && list.Count + num < teamMax)
            {
                list.Add(basicRole);
            }

            AssignRolesFromList(players, teamMax, list, ref num);
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Fallback Assignment");
        }
        else if (team == RoleTeamTypes.Impostor)
        {
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Before Guaranteed Assignment");
            var newImpostors = new List<NetworkedPlayerInfo>();
            // Specified Seeker
            if (HasImpostorPlayerID() &&
               ValidateImpostorPlayerID(players) &&
                !AmongUsClient.Instance.IsGamePublic)
            {
                NetworkedPlayerInfo networkedPlayerInfo = players.ToArray()
                    .First(p => p.PlayerId == ImpostorPlayerID());
                players.Remove(networkedPlayerInfo);
                newImpostors.Add(networkedPlayerInfo);
                Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Seeker is {networkedPlayerInfo.PlayerName}, ID: {networkedPlayerInfo.PlayerId}");
            }
            // Random Seeker
            else
            {
                int num2 = 0;
                while (num2 < teamMax && players.Count > 0)
                {
                    PseudoRandomList<NetworkedPlayerInfo> pseudoRandomList = new PseudoRandomList<NetworkedPlayerInfo>(AmongUsClient.Instance.GameId);
                    players.Do(x => pseudoRandomList.Add(x));
                    for (int i = 0; i < GameData.RoundsPlayedInSession; i++)
                    {
                        pseudoRandomList.PickRandom();
                    }
                    NetworkedPlayerInfo networkedPlayerInfo = pseudoRandomList.PickRandom();
                    players.Remove(networkedPlayerInfo);
                    newImpostors.Add(networkedPlayerInfo);
                    num2++;
                    Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: Seeker is {networkedPlayerInfo.PlayerName}, ID: {networkedPlayerInfo.PlayerId}");
                }
            }
            Error($"MiraAPI.Patches.Roles.LogicRoleSelectionHnsPatch - AssignRolesForTeam: After Guaranteed Assignment");
            foreach (RoleManager.RoleAssignmentData roleAssignmentData in guaranteedRoles.Select((x) =>
                         new RoleManager.RoleAssignmentData(x, roleOptions.GetNumPerGame(x.Role), 100)))
            {
                while (true)
                {
                    RoleManager.RoleAssignmentData roleAssignmentData2 = roleAssignmentData;
                    int count = roleAssignmentData2.Count;
                    roleAssignmentData2.Count = count - 1;
                    if (count <= 0)
                    {
                        break;
                    }

                    list.Add(roleAssignmentData.Role.Role);
                }
            }
            AssignRolesFromList(newImpostors, teamMax, list, ref num);

            var list2 = source.Where(x => !x.IsDead).Select(role =>
                new RoleManager.RoleAssignmentData(
                    role,
                    roleOptions.GetNumPerGame(role.Role),
                    roleOptions.GetChancePerGame(role.Role))).ToList();

            list.Clear();
            foreach (RoleManager.RoleAssignmentData roleAssignmentData3 in list2)
            {
                for (int i = 0; i < roleAssignmentData3.Count; i++)
                {
                    if (HashRandom.Next(101) < roleAssignmentData3.Chance)
                    {
                        list.Add(roleAssignmentData3.Role.Role);
                    }
                }
            }

            AssignRolesFromList(newImpostors, teamMax, list, ref num);
            var basicRole = RoleTypes.Impostor;
            while (list.Count < newImpostors.Count && list.Count + num < teamMax)
            {
                list.Add(basicRole);
            }

            AssignRolesFromList(newImpostors, teamMax, list, ref num);
        }
    }

    public static void AssignRolesFromList(List<NetworkedPlayerInfo> players, int teamMax, List<RoleTypes> roleList, ref int rolesAssigned)
    {
        while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
        {
            int index = HashRandom.FastNext(roleList.Count);
            RoleTypes roleType = roleList[index];
            roleList.RemoveAt(index);
            int index2 = HashRandom.FastNext(players.Count);
            players[index2].Object.RpcSetRole(roleType, false);
            players.RemoveAt(index2);
            rolesAssigned++;
        }
    }

    public override IEnumerator IntroCutscene(IntroCutscene __instance)
    {
        SoundManager.Instance.PlaySound(__instance.IntroStinger, false, 1f, null);
        Logger.GlobalInstance.Info("IntroCutscene :: CoBegin() :: Game Mode: Hide and Seek (MiraAPI)", null);
        __instance.LogPlayerRoleData();
        __instance.HideAndSeekPanels.SetActive(true);
        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            __instance.CrewmateRules.SetActive(false);
            __instance.ImpostorRules.SetActive(true);
        }
        else
        {
            __instance.CrewmateRules.SetActive(true);
            __instance.ImpostorRules.SetActive(false);
        }

        __instance.ImpostorName.gameObject.SetActive(true);
        __instance.ImpostorTitle.gameObject.SetActive(true);
        __instance.BackgroundBar.enabled = false;
        __instance.TeamTitle.gameObject.SetActive(false);
        var impostor = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.Data.Role.IsImpostor);
        if (impostor == null)
        {
            Logger.GlobalInstance.Error("IntroCutscene :: CoBegin() :: impostor is NULL", null);
        }

        GameManager.Instance.SetSpecialCosmetics(impostor);
        if (impostor != null)
        {
            __instance.ImpostorName.text = impostor.Data.PlayerName;
        }
        else
        {
            __instance.ImpostorName.text = "???";
        }

        yield return new WaitForSecondsRealtime(0.1f);
        if (impostor != null)
        {
            __instance.ImpostorTitle.text = impostor.Data.Role.GetRoleName();
        }
        PoolablePlayer playerSlot = null;
        if (impostor != null)
        {
            playerSlot = __instance.CreatePlayer(1, 1, impostor.Data, false);
            playerSlot.SetBodyType(PlayerBodyTypes.Normal);
            playerSlot.SetFlipX(false);
            playerSlot.transform.localPosition = __instance.impostorPos;
            playerSlot.transform.localScale = Vector3.one * __instance.impostorScale;
        }

        yield return ShipStatus.Instance.CosmeticsCache.PopulateFromPlayers();
        yield return new WaitForSecondsRealtime(6f);
        if (playerSlot != null)
        {
            playerSlot.gameObject.SetActive(false);
        }

        __instance.HideAndSeekPanels.SetActive(false);
        __instance.CrewmateRules.SetActive(false);
        __instance.ImpostorRules.SetActive(false);
        HnsMusicHandler.Instance.StartMusicWithIntro();
        var hideTimer = 10f;

        if (PlayerControl.LocalPlayer.Data.Role.IsImpostor)
        {
            __instance.HideAndSeekTimerText.gameObject.SetActive(true);
            PoolablePlayer poolablePlayer;
            AnimationClip anim;
            if (AprilFoolsMode.ShouldHorseAround())
            {
                poolablePlayer = __instance.HorseWrangleVisualSuit;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                anim = __instance.HnSSeekerSpawnHorseAnim;
                __instance.HorseWrangleVisualPlayer.SetBodyType(PlayerBodyTypes.Normal);
                __instance.HorseWrangleVisualPlayer.UpdateFromPlayerData(
                    PlayerControl.LocalPlayer.Data,
                    PlayerControl.LocalPlayer.CurrentOutfitType,
                    PlayerMaterial.MaskType.None,
                    false,
                    null,
                    false);
            }
            else if (AprilFoolsMode.ShouldLongAround())
            {
                poolablePlayer = __instance.HideAndSeekPlayerVisual;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.LongSeeker);
                anim = __instance.HnSSeekerSpawnLongAnim;
            }
            else
            {
                poolablePlayer = __instance.HideAndSeekPlayerVisual;
                poolablePlayer.gameObject.SetActive(true);
                poolablePlayer.SetBodyType(PlayerBodyTypes.Seeker);
                anim = __instance.HnSSeekerSpawnAnim;
            }

            poolablePlayer.SetBodyCosmeticsVisible(false);
            poolablePlayer.UpdateFromPlayerData(
                PlayerControl.LocalPlayer.Data,
                PlayerControl.LocalPlayer.CurrentOutfitType,
                PlayerMaterial.MaskType.None,
                false,
                null,
                false);
            SpriteAnim component = poolablePlayer.GetComponent<SpriteAnim>();
            poolablePlayer.gameObject.SetActive(true);
            poolablePlayer.ToggleName(false);
            component.Play(anim, 1f);
            while (hideTimer > 0f)
            {
                __instance.HideAndSeekTimerText.text = Mathf.RoundToInt(hideTimer).ToString();
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
                    impostor.AnimateCustom(__instance.HnSSeekerSpawnHorseInGameAnim);
                }
            }
            else if (AprilFoolsMode.ShouldLongAround())
            {
                if (impostor != null)
                {
                    impostor.AnimateCustom(__instance.HnSSeekerSpawnLongInGameAnim);
                }
            }
            else if (impostor != null)
            {
                impostor.AnimateCustom(__instance.HnSSeekerSpawnAnim);
                impostor.cosmetics.SetBodyCosmeticsVisible(false);
            }
        }
        ShipStatus.Instance.StartSFX();
        HnsMusicHandler.Instance.OnGameStart();
        HnsDangerMeter.Instance.OnGameStart();
        UnityEngine.Object.Destroy(__instance.gameObject);
    }
    public override void Initialize()
    {
        deadPlayerCount = 0;
        ShipStatus.Instance.BreakEmergencyButton();
        PlayerControl.LocalPlayer.SetKillTimer(0.01f);
        if (HudManager.InstanceExists)
        {
            HudManager.Instance.gameObject.AddComponent<HnsMusicHandler>();
            HudManager.Instance.gameObject.AddComponent<HideAndSeekHudHelper>();
            HudManager.Instance.gameObject.AddComponent<HnsDangerMeter>();
        }
    }
    public override MapOptions GetMapOptions()
    {
        MapOptions mapOptions = new MapOptions
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

    public override void CheckGameEnd(out bool runOriginal, LogicGameFlowNormal instance)
    {
        runOriginal = false;
        if (Helpers.GetAlivePlayers().Count(x => x.Data.Role.IsImpostor) == 0)
        {
            instance.Manager.RpcEndGame(GameOverReason.ImpostorDisconnect, !DataManager.Player.Ads.HasPurchasedAdRemoval);
        }
        if (Helpers.GetAlivePlayers().Count(x => !x.Data.Role.IsImpostor) != 0)
        {
            if (HideAndSeekHudHelper.Instance.AllTimersExpired())
            {
                instance.Manager.RpcEndGame(GameOverReason.HideAndSeek_CrewmatesByTimer, !DataManager.Player.Ads.HasPurchasedAdRemoval);
            }
            return;
        }
        instance.Manager.RpcEndGame(GameOverReason.HideAndSeek_ImpostorsByKills, !DataManager.Player.Ads.HasPurchasedAdRemoval);
    }

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
        else if (AprilFoolsMode.ShouldHorseAround())
        {
            if (player.Data.Role.IsImpostor)
            {
                return PlayerBodyTypes.Normal;
            }
            return PlayerBodyTypes.Horse;
        }
        else if (AprilFoolsMode.ShouldLongAround())
        {
            if (player.Data.Role.IsImpostor)
            {
                return PlayerBodyTypes.LongSeeker;
            }
            return PlayerBodyTypes.Long;
        }
        else
        {
            if (player.Data.Role.IsImpostor)
            {
                return PlayerBodyTypes.Seeker;
            }
            return PlayerBodyTypes.Normal;
        }
    }
    public override void UpdateTaskPanel(TaskPanelBehaviour instance)
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

        var yPos = 1.6f;
        var xPos = -instance.background.sprite.bounds.size.x * instance.background.transform.localScale.x;
        instance.closedPosition = new Vector3(xPos, yPos, instance.closedPosition.z);
        instance.openPosition = new Vector3(instance.openPosition.x, yPos, instance.openPosition.z);
        if (instance.open)
        {
            instance.timer = Mathf.Min(1f, instance.timer + Time.deltaTime / instance.animationTimeSeconds);
        }
        else
        {
            instance.timer = Mathf.Max(0f, instance.timer - Time.deltaTime / instance.animationTimeSeconds);
        }

        Vector3 relativePos = new(
            Mathf.SmoothStep(instance.closedPosition.x, instance.openPosition.x, instance.timer),
            yPos,
            instance.openPosition.z);
        instance.transform.localPosition =
            AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop, relativePos);
    }
    private int deadPlayerCount;
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
