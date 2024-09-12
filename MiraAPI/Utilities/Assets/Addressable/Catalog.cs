using BepInEx.Unity.IL2CPP.Utils.Collections;
using Reactor.Utilities;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MiraAPI.Utilities.Assets.Addressable;

/// <summary>
/// Catalogs are a representation of resource locators for the Unity Addressables api.
/// </summary>
public class Catalog(string CatalogPath)
{
    public void LoadCatalogAsync() {
        Coroutines.Start(LoadCatalogAsynchronously());
    }
    public void LoadCatalog()
    {
        Addressables.LoadContentCatalog(CatalogPath).WaitForCompletion();
    }
    protected IEnumerator LoadCatalogAsynchronously()
    {
        AsyncOperationHandle<IResourceLocator> handle = Addressables.LoadContentCatalogAsync(CatalogPath, true);
        yield return handle;
    }
    protected IEnumerator UpdateCatalog()
    {
        string[] toUpdate = { CatalogPath };
        var catalogs = toUpdate.WrapToIl2Cpp().Cast<Il2CppSystem.Collections.Generic.IEnumerable<string>>();
        var updateHandle = Addressables.UpdateCatalogs(catalogs);

        yield return updateHandle;
        updateHandle.Release();
    }
}