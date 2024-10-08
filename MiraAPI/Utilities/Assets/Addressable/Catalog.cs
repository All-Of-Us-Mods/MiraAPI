using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Reactor.Utilities;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MiraAPI.Utilities.Assets.Addressable;

/// <summary>
/// Catalogs are a representation of resource locators for the Unity Addressables api.
/// </summary>
public class Catalog(string CatalogPath)
{
    /// <summary>
    /// Loads the catalog asynchronously.
    /// </summary>
    public void LoadCatalogAsync()
    {
        Coroutines.Start(LoadCatalogAsynchronously());
    }

    /// <summary>
    /// Loads the catalog. This must be done before you can call any addressables.
    /// </summary>
    public void LoadCatalog()
    {
        Addressables.LoadContentCatalog(CatalogPath).WaitForCompletion();
    }

    /// <summary>
    /// Updates the network catalogs.
    /// </summary>
    public void UpdateNetworkCatalog()
    {
        Coroutines.Start(UpdateCatalog());
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
