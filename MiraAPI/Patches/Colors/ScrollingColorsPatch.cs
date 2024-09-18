using HarmonyLib;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Patches.Colors;

[HarmonyPatch(typeof(PlayerTab))]
public static class ScrollingColorsPatch
{
    // Collider
    private static BoxCollider2D? _collider;

    /// <summary>
    /// Add scrolling to the colors tab.
    /// </summary>
    [HarmonyPostfix, HarmonyPatch("OnEnable")]
    public static void AddScrollingToColorsTabPatch(PlayerTab __instance)
    {
        if (!PlayerCustomizationMenu.Instance)
        {
            return;
        }

        var tab = PlayerCustomizationMenu.Instance.Tabs[1].Tab;

        if (__instance.scroller == null)
        {
            var newScroller = Object.Instantiate(tab.scroller, __instance.transform, true);
            newScroller.Inner.transform.DestroyChildren();

            var maskObj = new GameObject
            {
                layer = 5,
                name = "SpriteMask",
            };
            maskObj.transform.SetParent(__instance.transform);
            maskObj.transform.localPosition = new Vector3(0, 0, 0);
            maskObj.transform.localScale = new Vector3(500, 4.76f);

            var mask = maskObj.AddComponent<SpriteMask>();
            mask.sprite = MiraAssets.Empty.LoadAsset();

            _collider = maskObj.AddComponent<BoxCollider2D>();
            _collider.size = new Vector2(1f, 0.75f);
            _collider.enabled = true;
            __instance.scroller = newScroller;
        }

        foreach (var chip in __instance.ColorChips)
        {
            chip.transform.SetParent(__instance.scroller.Inner);
            chip.Button.ClickMask = _collider;
            chip.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        __instance.SetScrollerBounds();
    }
}
