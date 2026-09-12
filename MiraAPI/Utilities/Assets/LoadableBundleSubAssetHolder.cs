using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for loading multiple assets from an <see cref="AssetBundle"/>.
/// </summary>
public class LoadableBundleSubAssetHolder
{
    private readonly string[] spriteNames;
    private readonly AssetBundle bundle;

    /// <summary>
    /// Gets the sprites contained within the asset.
    /// </summary>
    public Sprite[] SubSprites { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableBundleSubAssetHolder"/> class.
    /// </summary>
    /// <param name="names">The name of the assets to pull from.</param>
    /// <param name="bundle">The <see cref="AssetBundle"/> that contains the assets.</param>
    public LoadableBundleSubAssetHolder(string[] names, AssetBundle bundle)
    {
        this.bundle = bundle;
        spriteNames = names;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableBundleSubAssetHolder"/> class.
    /// </summary>
    /// <param name="name">The name of the asset.</param>
    /// <param name="bundle">The <see cref="AssetBundle"/> that contains the assets.</param>
    public LoadableBundleSubAssetHolder(string name, AssetBundle bundle)
    {
        this.bundle = bundle;
        spriteNames = [name];
    }

    /// <summary>
    /// Attempts to load all of the sprite assets within the sprite sheet.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the asset is not actually a sprite sheet.</exception>
    [SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "Null coalescing bypasses Unity lifetime checks.")]
    public void TryInit()
    {
        if (SubSprites.Length == 0)
        {
            var newSprites = Array.Empty<Sprite>();

            foreach (var name in spriteNames)
            {
                var loadedAssets = bundle.LoadAssetWithSubAssets(name, Il2CppType.From(typeof(Sprite))).ToArray();

                if (loadedAssets == null)
                {
                    throw new InvalidOperationException($"INVALID ASSETS: {name}");
                }

                foreach (var obj in loadedAssets)
                {
                    var img = obj.TryCast<Sprite>();
                    if (img != null)
                    {
                        img.DontDestroy().DontUnload();
                        newSprites = newSprites.AddToArray(img);
                    }
                }
            }

            SubSprites = newSprites;
        }
    }

    /// <summary>
    /// Unloads an asset.
    /// </summary>
    /// <returns>True if the asset was unloaded, false otherwise.</returns>
    public bool UnloadAsset()
    {
        if (SubSprites.Length == 0)
        {
            return false;
        }

        foreach (var sprite in SubSprites)
        {
            sprite.DestroyImmediate();
        }
        SubSprites = [];
        return true;
    }
}
