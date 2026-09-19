using System.Collections.Generic;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;
public static class TmpSpriteUtils
{
    private static Shader _spriteShader;
    private static Dictionary<string, TMP_SpriteAsset> LoadedSprites = [];
    public static TMP_SpriteAsset AssetHolder;
    public static TMP_SpriteAsset CreateSpriteAsset(Sprite sprite, string assetName, float scale = 1)
    {
        return CreateSpriteAsset(sprite.texture, sprite.rect, assetName, scale);
    }
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

        TMP_SpriteAsset spriteAssetMasked = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
        spriteAssetMasked.name = assetName + ".Masked";
        spriteAssetMasked.spriteSheet = sourceTexture;

        if (!_spriteShader)
        {
            _spriteShader = Shader.Find("TextMeshPro/Sprite");
        }

        var material = new Material(_spriteShader)
        {
            name = assetName + " Material",
        };
        material.SetTexture(ShaderUtilities.ID_MainTex, sourceTexture);
        /*material.SetFloat(ShaderUtilities.ID_StencilComp, 4);
        material.SetFloat(ShaderUtilities.ID_StencilID, 1);*/
        material.SetFloat(ShaderUtilities.ID_StencilComp, 0);
        material.SetFloat(ShaderUtilities.ID_StencilID, 0);
        material.SetFloat(ShaderUtilities.ID_StencilOp, 0);
        material.SetFloat(ShaderUtilities.ID_StencilWriteMask, 255);
        material.SetFloat(ShaderUtilities.ID_StencilReadMask, 255);
        material.SetFloat(ShaderUtilities.ShaderTag_CullMode, 0);
        spriteAsset.material = material;

        spriteAsset.spriteInfoList = new Il2CppSystem.Collections.Generic.List<TMP_Sprite>();

        var maskedMaterial = new Material(_spriteShader)
        {
            name = assetName + " Masked Material",
        };
        maskedMaterial.SetTexture(ShaderUtilities.ID_MainTex, sourceTexture);
        maskedMaterial.SetFloat(ShaderUtilities.ID_StencilComp, 4);
        maskedMaterial.SetFloat(ShaderUtilities.ID_StencilID, 1);
        maskedMaterial.SetFloat(ShaderUtilities.ID_StencilOp, 0);
        maskedMaterial.SetFloat(ShaderUtilities.ID_StencilWriteMask, 255);
        maskedMaterial.SetFloat(ShaderUtilities.ID_StencilReadMask, 255);
        maskedMaterial.SetFloat(ShaderUtilities.ShaderTag_CullMode, 0);
        spriteAssetMasked.material = maskedMaterial;

        spriteAssetMasked.spriteInfoList = new Il2CppSystem.Collections.Generic.List<TMP_Sprite>();

        AddSpriteToAsset(spriteAsset, rect, assetName, scale);
        AddSpriteToAsset(spriteAssetMasked, rect, assetName + ".Masked", scale);

        spriteAsset.DontUnload().DontDestroy();
        spriteAsset.UpdateLookupTables();
        spriteAsset.fallbackSpriteAssets = new();
        spriteAssetMasked.DontUnload().DontDestroy();
        spriteAssetMasked.UpdateLookupTables();
        spriteAssetMasked.fallbackSpriteAssets = new();
        AssetHolder.fallbackSpriteAssets.Add(spriteAsset);
        AssetHolder.fallbackSpriteAssets.Add(spriteAssetMasked);
        AssetHolder.UpdateLookupTables();

        LoadedSprites.Add(assetName, spriteAsset);
        LoadedSprites.Add(assetName + ".Masked", spriteAssetMasked);
        return spriteAsset;
    }

    private static void AddSpriteToAsset(TMP_SpriteAsset spriteAsset, Rect rect, string spriteName, float scale)
    {
        TMP_Sprite newSprite = new TMP_Sprite
        {
            name = spriteName,
            hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteName),
            x = rect.x,
            y = rect.y,
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
