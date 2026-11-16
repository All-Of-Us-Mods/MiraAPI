using System;
using System.Collections.Generic;
using System.Reflection;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A utility class for various sprite-related operations.
/// </summary>
public static class SpriteTools
{
    private static readonly Dictionary<(Assembly Assembly, string ResourcePath), Texture2D> TextureCache = [];
    private static readonly Dictionary<(Assembly Assembly, string ResourcePath, float PixelsPerUnit), Sprite> SpriteCache = [];

    /// <summary>
    /// Loads and returns a texture from a resource path using the specified <see cref="Assembly"/>.
    /// </summary>
    /// <param name="resourcePath">The path to the resource within the <see cref="Assembly"/>.</param>
    /// <param name="assembly">The <see cref="Assembly"/> from which to load the resource.</param>
    /// <returns>A <see cref="Texture2D"/> object loaded from the specified resource path.</returns>
    /// <exception cref="ArgumentException">Thrown when the resource cannot be found in the specified <see cref="Assembly"/>.</exception>
    public static Texture2D LoadTextureFromResourcePath(string resourcePath, Assembly assembly)
    {
        if (TextureCache.TryGetValue((assembly, resourcePath), out var cachedTexture) && cachedTexture != null)
        {
            return cachedTexture;
        }

        var tex = new Texture2D(1, 1, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
        };
        using var myStream = assembly.GetManifestResourceStream(resourcePath);
        if (myStream != null)
        {
            var buttonTexture = myStream.ReadFully();
            tex.LoadImage(buttonTexture, false);
        }
        else
        {
            throw new ArgumentException($"Resource not found: {resourcePath}");
        }

        tex.name = resourcePath;
        TextureCache[(assembly, resourcePath)] = tex;
        return tex;
    }

    /// <summary>
    /// Loads and returns a <see cref="Sprite"/> from a resource path using the specified <see cref="Assembly"/>.
    /// </summary>
    /// <param name="resourcePath">The path to the resource within the <see cref="Assembly"/>.</param>
    /// <param name="assembly">The <see cref="Assembly"/> from which to load the resource.</param>
    /// <param name="pixelsPerUnit">The number of pixels per unit for the sprite.</param>
    /// <returns>A <see cref="Sprite"/> object created from the texture loaded from the specified resource path.</returns>
    public static Sprite LoadSpriteFromPath(string resourcePath, Assembly assembly, float pixelsPerUnit)
    {
        if (SpriteCache.TryGetValue((assembly, resourcePath, pixelsPerUnit), out var cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        var tex = LoadTextureFromResourcePath(resourcePath, assembly);
        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit,
            0,
            SpriteMeshType.FullRect);
        sprite.name = resourcePath;
        SpriteCache[(assembly, resourcePath, pixelsPerUnit)] = sprite;
        return sprite;
    }
}
