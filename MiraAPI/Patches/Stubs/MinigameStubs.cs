using System.Runtime.CompilerServices;
using HarmonyLib;

namespace MiraAPI.Patches.Stubs;

/// <summary>
/// Reverse patches for Minigames.
/// </summary>
[HarmonyPatch]
public static class MinigameStubs
{
    /// <summary>
    /// Reverse patch for Minigame.Begin.
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
}
