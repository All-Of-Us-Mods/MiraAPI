using Il2CppInterop.Runtime.Attributes;
using MiraAPI.Patches.Menu;
using Reactor.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// The component for loading Addressables, and cosmetics.
/// </summary>
public static class AddressablesLoader
{
    private static bool _isInitialized;

    private static readonly List<(string Location, string ProviderSuffix)> CatalogLocations = [];
    private static readonly List<string> LoadedLocations = [];

    private static readonly List<string> RegisteredHatKeys = [];
    private static readonly List<(string, string)> RegisteredVisorKeys = [];
    private static readonly List<(string, string)> RegisteredNameplateKeys = [];
    private static readonly List<string> RegisteredSkinKeys = [];

    /// <summary>
    /// Gets a value indicating whether hats have been loaded by the addressables system.
    /// </summary>
    public static bool AddressableHatsExist { get; private set; }

    /// <summary>
    /// Gets a value indicating whether visors have been loaded by the addressables system.
    /// </summary>
    public static bool AddressableVisorsExist { get; private set; }

    /// <summary>
    /// Gets a value indicating whether nameplates have been loaded by the addressables system.
    /// </summary>
    public static bool AddressableNameplatesExist { get; private set; }

    /// <summary>
    /// Gets a value indicating whether skins have been loaded by the addressables system.
    /// </summary>
    public static bool AddressableSkinsExist { get; private set; }

    /// <summary>
    /// Registers a specific addressables package to load asynchronously at the start of the game, when possible.
    /// </summary>
    /// <param name="location">The location, remote or otherwise, of the addressables.</param>
    /// <param name="providerSuffix">The suffix of the provider for an addressables package.</param>
    public static void RegisterCatalog(string location, string providerSuffix = "")
    {
        if (_isInitialized)
        {
            Error("AddressablesLoader has already been initialized, cannot register more catalogs.");
            return;
        }

        CatalogLocations.Add((location, providerSuffix));
    }

    /// <summary>
    /// Registers a specific addressables key as only containing <see cref="HatData"/>'s to load.
    /// </summary>
    /// <param name="addressablesKey">The key/label/group for a List <see cref="HatData"/>.</param>
    public static void RegisterHats(string addressablesKey)
    {
        if (_isInitialized)
        {
            Error("AddressablesLoader has already been initialized, cannot register more keys for hats.");
            return;
        }

        AddressableHatsExist = true;
        RegisteredHatKeys.Add(addressablesKey);
    }

    /// <summary>
    /// Registers a specific addressables key as only containing <see cref="SkinData"/>'s to load.
    /// </summary>
    /// <param name="addressablesKey">The key/label/group for a List <see cref="SkinData"/>.</param>
    public static void RegisterSkins(string addressablesKey)
    {
        if (_isInitialized)
        {
            Error("AddressablesLoader has already been initialized, cannot register more keys for skins.");
            return;
        }

        AddressableSkinsExist = true;
        RegisteredSkinKeys.Add(addressablesKey);
    }

    /// <summary>
    /// Registers a specific addressables key as only containing <see cref="NamePlateData"/>'s to load.
    /// </summary>
    /// <param name="addressablesKey">The key/label/group for a List <see cref="NamePlateData"/>.</param>
    /// /// <param name="groupTitle">The title of the group for visors.</param>
    public static void RegisterNameplates(string addressablesKey, string groupTitle)
    {
        if (_isInitialized)
        {
            Error("AddressablesLoader has already been initialized, cannot register more keys for nameplates.");
            return;
        }

        AddressableNameplatesExist = true;
        RegisteredNameplateKeys.Add((addressablesKey, groupTitle));
    }

    /// <summary>
    /// Registers a specific addressables key as only containing <see cref="VisorData"/>'s to load.
    /// </summary>
    /// <param name="addressablesKey">The key/label/group for a List <see cref="VisorData"/>.</param>
    /// <param name="groupTitle">The title of the group for visors.</param>
    public static void RegisterVisors(string addressablesKey, string groupTitle)
    {
        if (_isInitialized)
        {
            Error("AddressablesLoader has already been initialized, cannot register more keys for visors.");
            return;
        }

        AddressableVisorsExist = true;
        RegisteredVisorKeys.Add((addressablesKey, groupTitle));
    }

    internal static void LoadAll()
    {
        if (_isInitialized)
        {
            Error("AddressablesLoader has already been initialized.");
            return;
        }

        _isInitialized = true;
        foreach (var (location, providerSuffix) in CatalogLocations)
        {
            Coroutines.Start(CoLoadAddressables(location, providerSuffix));
        }

        Coroutines.Start(LoadCosmetics());
    }

    internal static IEnumerator CoLoadAddressables(string location, string suffix = "")
    {
        while (!AmongUsClient.Instance) yield return null;
        // Load the local/remote content catalog
        var catalogOperation = Addressables.LoadContentCatalog(location, suffix);
        yield return catalogOperation;

        // Check for errors
        if (catalogOperation.Status != AsyncOperationStatus.Succeeded)
        {
            Error($"Failed to load catalog {location}.");
        }

        Info($"Loaded addressables {location}.");
        LoadedLocations.Add(location);
    }

    [HideFromIl2Cpp]
    internal static IEnumerator LoadCosmetics()
    {
        while (!AmongUsClient.Instance || CatalogLocations.Select(x => x.Location).Any(x => !LoadedLocations.Contains(x))) yield return null;

        var hatBehaviours = DiscoverData<HatData>(RegisteredHatKeys);
        hatBehaviours = [.. hatBehaviours.OrderBy(x => x.StoreName)];
        var skinBehaviours = DiscoverData<SkinData>(RegisteredSkinKeys);
        skinBehaviours = [.. skinBehaviours.OrderBy(x => x.StoreName)];
        var namePlateBehaviours = DiscoverAndReportData<NamePlateData>(RegisteredNameplateKeys);
        namePlateBehaviours = [.. namePlateBehaviours.OrderBy(x => x.Category)];
        var visorBehaviours = DiscoverAndReportData<VisorData>(RegisteredVisorKeys);
        visorBehaviours = [.. visorBehaviours.OrderBy(x => x.Category)];

        var hatData = new List<HatData>();
        hatData.AddRange(HatManager.Instance.allHats);
        hatData.ForEach(x => x.StoreName = "Vanilla");
        HatManager.Instance.allHats = PrepareArray(hatData, hatBehaviours);

        var skinData = new List<SkinData>();
        skinData.AddRange(HatManager.Instance.allSkins);
        skinData.ForEach(x => x.StoreName = "Vanilla");
        HatManager.Instance.allSkins = PrepareArray(skinData, skinBehaviours);

        var visorData = new List<VisorData>();
        visorData.AddRange(HatManager.Instance.allVisors);
        VisorsTabPatches.AddRange(visorBehaviours);
        HatManager.Instance.allVisors = PrepareArray(visorData, [.. visorBehaviours.Select(x => x.Data)]);

        var namePlateData = new List<NamePlateData>();
        namePlateData.AddRange(HatManager.Instance.allNamePlates);
        NameplatesTabPatches.AddRange(namePlateBehaviours);
        HatManager.Instance.allNamePlates = PrepareArray(namePlateData, [.. namePlateBehaviours.Select(x => x.Data)]);
    }

    private static T[] PrepareArray<T>(List<T> data, List<T> behaviours) where T : CosmeticData
    {
        var count = data.Count;
        for (var i = 0; i < behaviours.Count; i++)
        {
            behaviours[i].displayOrder = count + i;
            data.Add(behaviours[i]);
        }
        return [.. data];
    }

    private static List<T> DiscoverData<T>(List<string> tags)
    {
        var behaviours = new List<T>();

        foreach (var tag in tags)
        {
            try
            {
                var allLocations = Addressables.LoadResourceLocationsAsync(tag).WaitForCompletion();
                var assets = Addressables.LoadAssetsAsync<T>(allLocations, null, false).WaitForCompletion();
                var array = new Il2CppSystem.Collections.Generic.List<T>(assets.Pointer);
                behaviours.AddRange(array.ToArray());
            }
            catch
            {
                Error($"Failed to find tag {tag}");
            }
        }
        return behaviours;
    }

    private static List<(string Category, T Data)> DiscoverAndReportData<T>(List<(string Tag, string Category)> tags)
    {
        var behaviours = new List<(string, T)>();

        foreach (var tag in tags)
        {
            try
            {
                var allLocations = Addressables.LoadResourceLocationsAsync(tag.Tag).WaitForCompletion();
                var assets = Addressables.LoadAssetsAsync<T>(allLocations, null, false).WaitForCompletion();
                var array = new Il2CppSystem.Collections.Generic.List<T>(assets.Pointer);
                behaviours.AddRange(array.ToArray().Select(x => (tag.Category, x)));
            }
            catch
            {
                Error($"Failed to find tag {tag}");
            }
        }
        return behaviours;
    }
}
