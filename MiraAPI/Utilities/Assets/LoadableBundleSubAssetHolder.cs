using System;
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
    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableBundleSubAssetHolder"/> class.
    /// </summary>
    /// <param name="names">The name of the assets to pull from.</param>
    /// <param name="bundle">The <see cref="AssetBundle"/> that contains the assets.</param>
    public LoadableBundleSubAssetHolder(string[] names, AssetBundle bundle)
    {
        Bundle = bundle;
        SpriteNames = names;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadableBundleSubAssetHolder"/> class.
    /// </summary>
    /// <param name="name">The name of the asset.</param>
    /// <param name="bundle">The <see cref="AssetBundle"/> that contains the assets.</param>
    public LoadableBundleSubAssetHolder(string name, AssetBundle bundle)
    {
        Bundle = bundle;
        SpriteNames = [name];
    }
    internal string[] SpriteNames = [];
    internal AssetBundle Bundle;
    public Sprite[] SubSprites = [];

    public void TryInit()
    {
        if (SubSprites.Length == 0)
        {
            var newSprites = Array.Empty<Sprite>();
            foreach (var name in SpriteNames)
            {
                var loadedAssets = Bundle.LoadAssetWithSubAssets(name, Il2CppType.From(typeof(Sprite))).ToArray();

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
