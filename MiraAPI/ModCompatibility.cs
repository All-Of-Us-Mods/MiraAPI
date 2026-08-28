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

    public static Version SubVersion { get; private set; }
    public static bool SubLoaded { get; private set; }
    public static BasePlugin SubPlugin { get; private set; }
    public static Assembly SubAssembly { get; private set; }

    public const ShipStatus.MapType SubmergedMapType = (ShipStatus.MapType)6;

    public static void Initialize()
    {
        InitSubmerged();
    }

    public static void InitSubmerged()
    {
        if (!IL2CPPChainloader.Instance.Plugins.TryGetValue(SubmergedId, out var plugin))
        {
            return;
        }

        SubPlugin = (plugin!.Instance as BasePlugin)!;
        SubVersion = plugin.Metadata.Version;

        SubAssembly = SubPlugin.GetType().Assembly;

        SubLoaded = true;
        Message("Submerged was detected");
    }

    public static bool IsSubmerged()
    {
        return SubLoaded && ShipStatus.Instance && ShipStatus.Instance.Type == SubmergedMapType;
    }
}
