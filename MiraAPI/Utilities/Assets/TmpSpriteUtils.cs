using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// Utility class for dynamically generating and managing TextMeshPro sprite assets.
/// </summary>
public static class TmpSpriteUtils
{
    private static Shader _spriteShader;
    private static readonly Dictionary<string, TMP_SpriteAsset> LoadedSprites = [];

    [SuppressMessage("Critical Code Smell", "S2223:Non-constant static fields should not be visible", Justification = "Internal behaviour that does not need property-level validation.")]
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
    internal static TMP_SpriteAsset AssetHolder;

    /// <summary>
    /// Creates and caches a <see cref="TMP_SpriteAsset"/> from a given Unity <see cref="Sprite"/>.
    /// </summary>
    /// <param name="sprite">The Unity sprite to convert into a TMP sprite asset.</param>
    /// <param name="assetName">The unique name for the sprite asset, used for caching and inline rich text tags.</param>
    /// <param name="scale">The scale applied to the sprite. Defaults to 1.</param>
    /// <returns>The generated or previously cached <see cref="TMP_SpriteAsset"/>.</returns>
    public static TMP_SpriteAsset CreateSpriteAsset(Sprite sprite, string assetName, float scale = 1)
    {
        return CreateSpriteAsset(sprite.texture, sprite.rect, assetName, scale);
    }

    /// <summary>
    /// Creates and caches a <see cref="TMP_SpriteAsset"/> from a source texture and a specific region.
    /// </summary>
    /// <param name="sourceTexture">The source <see cref="Texture2D"/> containing the sprite.</param>
    /// <param name="rect">The specific region of the texture to use.</param>
    /// <param name="assetName">The unique name for the sprite asset, used for caching and inline rich text tags.</param>
    /// <param name="scale">The scale applied to the sprite. Defaults to 1.</param>
    /// <returns>The generated or previously cached <see cref="TMP_SpriteAsset"/>.</returns>
    public static TMP_SpriteAsset CreateSpriteAsset(Texture2D sourceTexture, Rect rect, string assetName, float scale = 1)
    {
        if (LoadedSprites.TryGetValue(assetName, out var existingAsset))
        {
            return existingAsset;
        }

        if (!AssetHolder)
        {
            AssetHolder = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            AssetHolder.DontUnload().DontDestroy();
            AssetHolder.fallbackSpriteAssets = new();
        }

        TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
        spriteAsset.name = assetName;
        spriteAsset.spriteSheet = sourceTexture;

        if (!_spriteShader)
        {
            _spriteShader = Shader.Find("TextMeshPro/Sprite");
        }

        var material = new Material(_spriteShader)
        {
            name = assetName + " Material",
        };
        material.SetTexture(ShaderUtilities.ID_MainTex, sourceTexture);
        // TODO: Using these values, the icons will NOT clip through chat boxes. However, this breaks quite literally every other tmp text that isn't masked. Please fix this if a good solution is found.
#pragma warning disable S125 // Sections of code should not be commented out
        /*material.SetFloat(ShaderUtilities.ID_StencilComp, 4);
        material.SetFloat(ShaderUtilities.ID_StencilID, 1);*/
#pragma warning restore S125 // Sections of code should not be commented out
        material.SetFloat(ShaderUtilities.ID_StencilComp, 0);
        material.SetFloat(ShaderUtilities.ID_StencilID, 0);
        material.SetFloat(ShaderUtilities.ID_StencilOp, 0);
        material.SetFloat(ShaderUtilities.ID_StencilWriteMask, 255);
        material.SetFloat(ShaderUtilities.ID_StencilReadMask, 255);
        material.SetFloat(ShaderUtilities.ShaderTag_CullMode, 0);
        spriteAsset.material = material;

        spriteAsset.spriteInfoList = new Il2CppSystem.Collections.Generic.List<TMP_Sprite>();

        AddSpriteToAsset(spriteAsset, rect, assetName, scale);

        spriteAsset.DontUnload().DontDestroy();
        spriteAsset.UpdateLookupTables();
        spriteAsset.fallbackSpriteAssets = new();
        AssetHolder.fallbackSpriteAssets.Add(spriteAsset);
        AssetHolder.UpdateLookupTables();

        LoadedSprites.Add(assetName, spriteAsset);
        return spriteAsset;
    }

    /// <summary>
    /// Constructs a <see cref="TMP_Sprite"/> with calculated offsets and adds it to the provided <see cref="TMP_SpriteAsset"/>.
    /// </summary>
    /// <param name="spriteAsset">The target sprite asset to add the sprite information to.</param>
    /// <param name="rect">The texture rect dimensions used to calculate width, height, and offsets.</param>
    /// <param name="spriteName">The name assigned to the sprite, used to generate its hash code.</param>
    /// <param name="scale">The visual scale applied to the sprite rendering.</param>
    private static void AddSpriteToAsset(TMP_SpriteAsset spriteAsset, Rect rect, string spriteName, float scale)
    {
        var newSprite = new TMP_Sprite
        {
            name = spriteName,
            hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteName),
            x = 0,
            y = 0,
            width = rect.width,
            height = rect.height,
            xOffset = -(rect.width / 3) + (scale - 1) * (rect.width / 2),
            yOffset = rect.height / 1.25f,
            xAdvance = rect.width,
            scale = scale,
        };

        spriteAsset.spriteInfoList.Add(newSprite);
    }
}
