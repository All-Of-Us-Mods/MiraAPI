using System.Reflection;
using BepInEx.Unity.IL2CPP;
using SemanticVersioning;

namespace MiraAPI;

/// <summary>
/// Mod compatibility tools.
/// </summary>
public static class ModCompatibility
{
    /// <summary>
    /// The ID for the Submerged mod.
    /// </summary>
    public const string SubmergedId = "Submerged";

    /// <summary>
    /// Gets the <see cref="Version"/> of the Submerged mod.
    /// </summary>
    public static Version SubVersion { get; private set; }

    /// <summary>
    /// Gets a value indicating whether Submerged is loaded.
    /// </summary>
    public static bool SubLoaded { get; private set; }

    /// <summary>
    /// Gets the <see cref="BasePlugin"/> of the Submerged mod.
    /// </summary>
    public static BasePlugin SubPlugin { get; private set; }

    /// <summary>
    /// Gets the <see cref="Assembly"/> of the Submerged mod.
    /// </summary>
    public static Assembly SubAssembly { get; private set; }

    /// <summary>
    /// The map ID of Submerged.
    /// </summary>
    public const ShipStatus.MapType SubmergedMapType = (ShipStatus.MapType)6;

    internal static void Initialize()
    {
        InitSubmerged();
    }

    private static void InitSubmerged()
    {
        if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(SubmergedId, out var plugin))
        {
            return;
        }

        SubPlugin = (plugin.Instance as BasePlugin)!;
        SubVersion = plugin.Metadata.Version;

        SubAssembly = SubPlugin.GetType().Assembly;

        SubLoaded = true;
        Message("Submerged was detected");
    }

    /// <summary>
    /// Checks to see if the current map is the Submerged map.
    /// </summary>
    /// <returns>true if the current map is Submerged.</returns>
    public static bool IsSubmerged()
    {
        return SubLoaded && ShipStatus.Instance && ShipStatus.Instance.Type == SubmergedMapType;
    }
}
