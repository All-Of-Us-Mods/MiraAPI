using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using HarmonyLib;

namespace MiraAPI.Patches.Stubs;

/// <summary>
/// Reverse patches for <see cref="Minigame"/>s.
/// </summary>
[HarmonyPatch]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Stub parameters.")]
public static class MinigameStubs
{
    /// <summary>
    /// Reverse patch for <see cref="Minigame.Begin"/>.
    /// </summary>
    /// <param name="instance">The Minigame instance.</param>
    /// <param name="task">Associated PlayerTask.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Begin(Minigame instance, PlayerTask? task)
    {
        // nothing needed
    }

    /// <summary>
    /// Reverse patch for <see cref="Minigame.Close()"/>.
    /// </summary>
    /// <param name="instance">The Minigame instance.</param>
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), [])]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Close(Minigame instance)
    {
        // nothing needed
    }
}
