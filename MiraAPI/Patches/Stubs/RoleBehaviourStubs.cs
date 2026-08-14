using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Hazel;
using Il2CppSystem.Text;

namespace MiraAPI.Patches.Stubs;

/// <summary>
/// Stub methods for the <see cref="RoleBehaviour"/> class. Needed because of Il2Cpp limitations on injected classes.
/// </summary>
[HarmonyPatch]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Stub parameters.")]
public static class RoleBehaviourStubs
{
    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.Initialize"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="player">The <see cref="PlayerControl"/> to initialize.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.Initialize))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Initialize(RoleBehaviour instance, PlayerControl player)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.AdjustTasks"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="player">The <see cref="PlayerControl"/> to adjust tasks for.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.AdjustTasks))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AdjustTasks(RoleBehaviour instance, PlayerControl player)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.AppendTaskHint(StringBuilder)"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="taskStringBuilder">The <see cref="StringBuilder"/> to append the task hint to.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.AppendTaskHint), typeof(StringBuilder))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AppendTaskHint(RoleBehaviour instance, StringBuilder taskStringBuilder)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.CanUse"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="console">The <see cref="IUsable"/> console to check.</param>
    /// <returns>Whether the console can be used.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.CanUse))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool CanUse(RoleBehaviour instance, IUsable console)
    {
        return false;
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.Deinitialize"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="targetPlayer">The <see cref="PlayerControl"/> to deinitialize for.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.Deinitialize))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Deinitialize(RoleBehaviour instance, PlayerControl targetPlayer)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.DidWin"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="gameOverReason">The reason for game over.</param>
    /// <returns>Whether the role won.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.DidWin))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool DidWin(RoleBehaviour instance, GameOverReason gameOverReason)
    {
        return false;
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.FindClosestTarget"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <returns>The closest target <see cref="PlayerControl"/>.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.FindClosestTarget))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static PlayerControl FindClosestTarget(RoleBehaviour instance)
    {
        return null!;
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.GetAbilityDistance"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <returns>The ability distance.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.GetAbilityDistance))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static float GetAbilityDistance(RoleBehaviour instance)
    {
        return 0;
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.IsValidTarget"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="target">The <see cref="NetworkedPlayerInfo"/> to validate.</param>
    /// <returns>Whether the target is valid.</returns>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.IsValidTarget))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static bool IsValidTarget(RoleBehaviour instance, NetworkedPlayerInfo target)
    {
        return false;
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.OnDeath"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="reason">The reason for death.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnDeath))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnDeath(RoleBehaviour instance, DeathReason reason)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.OnMeetingStart"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnMeetingStart))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnMeetingStart(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.OnVotingComplete"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnVotingComplete))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnVotingComplete(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.SetCooldown"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.SetCooldown))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetCooldown(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.SetPlayerTarget"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="target">The <see cref="PlayerControl"/> target to set.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.SetPlayerTarget))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetPlayerTarget(RoleBehaviour instance, PlayerControl target)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.SetUsableTarget"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="target">The <see cref="IUsable"/> target to set.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.SetUsableTarget))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SetUsableTarget(RoleBehaviour instance, IUsable target)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.SpawnTaskHeader"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="playerControl">The <see cref="PlayerControl"/> to spawn the task header for.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.SpawnTaskHeader))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void SpawnTaskHeader(RoleBehaviour instance, PlayerControl playerControl)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.UseAbility"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.UseAbility))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void UseAbility(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.UseSecondaryAbility"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.UseSecondaryAbility))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void UseSecondaryAbility(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.OnRoleSet"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.OnRoleSet))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void OnRoleSet(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.InitializeMeetingAbilityButton"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.InitializeMeetingAbilityButton))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void InitializeMeetingAbilityButton(RoleBehaviour instance)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.FindClosestBody"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.FindClosestBody))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static DeadBody FindClosestBody(RoleBehaviour instance)
    {
        return null!;
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.HandleRoleRpc"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="callId">The rpc message id.</param>
    /// <param name="reader">The <see cref="MessageReader"/> used to parse the message.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.HandleRoleRpc))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void HandleRoleRpc(RoleBehaviour instance, byte callId, MessageReader reader)
    {
        // nothing needed
    }

    /// <summary>
    /// Stub method for <see cref="RoleBehaviour.KillAnimSpecialSetup"/>.
    /// </summary>
    /// <param name="instance">The <see cref="RoleBehaviour"/> object.</param>
    /// <param name="deadBody">The <see cref="DeadBody"/> to apply the special setup to.</param>
    /// <param name="killer">The killer.</param>
    /// <param name="victim">The victim.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.KillAnimSpecialSetup))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void KillAnimSpecialSetup(RoleBehaviour instance, DeadBody deadBody, PlayerControl killer, PlayerControl victim)
    {
        // nothing needed
    }
}
