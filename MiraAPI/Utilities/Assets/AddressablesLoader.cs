using System;
using System.Diagnostics.CodeAnalysis;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// The component for loading Addressables, and cosmetics.
/// </summary>
[Obsolete("This system has been deprecated in favour of CorsacCosmetics. This will be removed in a future version.", true)]
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Part of obsolete code.")]
public static class AddressablesLoader
{
    /// <summary>
    /// Gets a value indicating whether hats have been loaded by the addressables system.
    /// </summary>
    public static bool AddressableHatsExist => false;

    /// <summary>
    /// Gets a value indicating whether visors have been loaded by the addressables system.
    /// </summary>
    public static bool AddressableVisorsExist => false;

    /// <summary>
    /// Gets a value indicating whether nameplates have been loaded by the addressables system.
    /// </summary>
    public static bool AddressableNameplatesExist => false;

    /// <summary>
    /// Gets a value indicating whether skins have been loaded by the addressables system.
    /// </summary>
    public static bool AddressableSkinsExist => false;

    /// <summary>
    /// Gets a value indicating whether catalogs have been loaded by the addressables system.
    /// </summary>
    public static bool CatalogsExist => false;

    /// <summary>
    /// Registers a specific addressables package to load asynchronously at the start of the game, when possible.
    /// </summary>
    /// <param name="location">The location, remote or otherwise, of the addressables.</param>
    /// <param name="providerSuffix">The suffix of the provider for an addressables package.</param>
    public static void RegisterCatalog(string location, string providerSuffix = "")
    {
        Warning("The addressables system has been deprecated. None of the registered assets will be added.");
    }

    /// <summary>
    /// Registers a specific addressables key as only containing <see cref="HatData"/>'s to load.
    /// </summary>
    /// <param name="addressablesKey">The key/label/group for a List <see cref="HatData"/>.</param>
    public static void RegisterHats(string addressablesKey)
    {
        Warning("The addressables system has been deprecated. None of the registered assets will be added.");
    }

    /// <summary>
    /// Registers a specific addressables key as only containing <see cref="SkinData"/>'s to load.
    /// </summary>
    /// <param name="addressablesKey">The key/label/group for a List <see cref="SkinData"/>.</param>
    public static void RegisterSkins(string addressablesKey)
    {
        Warning("The addressables system has been deprecated. None of the registered assets will be added.");
    }

    /// <summary>
    /// Registers a specific addressables key as only containing <see cref="NamePlateData"/>'s to load.
    /// </summary>
    /// <param name="addressablesKey">The key/label/group for a List <see cref="NamePlateData"/>.</param>
    /// /// <param name="groupTitle">The title of the group for visors.</param>
    public static void RegisterNameplates(string addressablesKey, string groupTitle)
    {
        Warning("The addressables system has been deprecated. None of the registered assets will be added.");
    }

    /// <summary>
    /// Registers a specific addressables key as only containing <see cref="VisorData"/>'s to load.
    /// </summary>
    /// <param name="addressablesKey">The key/label/group for a List <see cref="VisorData"/>.</param>
    /// <param name="groupTitle">The title of the group for visors.</param>
    public static void RegisterVisors(string addressablesKey, string groupTitle)
    {
        Warning("The addressables system has been deprecated. None of the registered assets will be added.");
    }
}
