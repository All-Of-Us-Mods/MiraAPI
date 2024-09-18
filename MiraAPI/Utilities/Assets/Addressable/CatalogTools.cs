using System.IO;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using UnityEngine.AddressableAssets;

namespace MiraAPI.Utilities.Assets.Addressable;

/// <summary>
/// A utility class for various catalog-related operations.
/// </summary>
public static class CatalogTools
{
    /// <summary>
    /// Converts string filenames to catalog paths within the plugins folder.
    /// </summary>
    /// <param name="fileName">A string of the catalog file without the extension.</param>
    /// <returns>The path to the catalog file.</returns>
    public static string ToLocalPath(this string fileName)
    {
        return Path.GetDirectoryName(typeof(MiraApiPlugin).Assembly.Location) + $"\\{fileName}.catalog";
    }

    /// <summary>
    /// Finds the resource locations of a specific label.
    /// </summary>
    /// <param name="label">The label of the addressables assets.</param>
    /// <returns>An IList of IResourceLocation.</returns>
    public static Il2CppSystem.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> GetResourceLocation(string label)
    {
        return Addressables.LoadResourceLocationsAsync(label).WaitForCompletion();
    }

    /// <summary>
    /// Finds the resource locations of multiple specific labels.
    /// </summary>
    /// <param name="labels">The label of the addressables assets.</param>
    /// <returns>An IList of IResourceLocation.</returns>
    public static Il2CppSystem.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> GetResourceLocations(string[] labels)
    {
        var il2CPPLabels = new Il2CppSystem.Collections.Generic.IList<Il2CppSystem.Object>(labels.WrapToIl2Cpp().Pointer);
        return Addressables.LoadResourceLocationsAsync(il2CPPLabels, Addressables.MergeMode.Union).WaitForCompletion();
    }
}
