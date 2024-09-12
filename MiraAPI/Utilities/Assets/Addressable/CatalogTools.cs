using BepInEx.Unity.IL2CPP.Utils.Collections;
using System.IO;
using System.Reflection;
using UnityEngine.AddressableAssets;

namespace MiraAPI.Utilities.Assets.Addressable;
public static class CatalogTools
{
    public static string ToLocalPath(this string fileName)
    {
        return Path.GetDirectoryName(Assembly.GetAssembly(typeof(MiraApiPlugin)).Location) + $"\\{fileName}.catalog";
    }

    public static Il2CppSystem.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> GetResourceLocation(string label)
    {
        return Addressables.LoadResourceLocationsAsync(label).WaitForCompletion();
    }

    public static Il2CppSystem.Collections.Generic.IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation> GetResourceLocations(string[] labels)
    {
        var il2cppLabels = new Il2CppSystem.Collections.Generic.IList<Il2CppSystem.Object>(labels.WrapToIl2Cpp().Pointer);
        return Addressables.LoadResourceLocationsAsync(il2cppLabels, Addressables.MergeMode.Union).WaitForCompletion();
    }
}
