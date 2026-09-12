using HarmonyLib;
using MiraAPI.Utilities.Assets;
using TMPro;

namespace MiraAPI.Patches;

[HarmonyPatch]
public static class TmpAwakePatch
{
    [HarmonyPatch(typeof(TextMeshPro), nameof(TextMeshPro.Awake))]
    [HarmonyPostfix]
    public static void TmpAwakePostfix(TextMeshPro __instance)
    {
        if (!TmpSpriteUtils.AssetHolder || __instance.m_spriteAsset == TmpSpriteUtils.AssetHolder)
        {
            return;
        }

        if (!__instance.m_spriteAsset)
        {
            __instance.m_spriteAsset = TmpSpriteUtils.AssetHolder;
            __instance.UpdateMeshPadding();
            return;
        }
        __instance.m_spriteAsset.fallbackSpriteAssets.Add(TmpSpriteUtils.AssetHolder);
        __instance.UpdateMeshPadding();
    }
}
