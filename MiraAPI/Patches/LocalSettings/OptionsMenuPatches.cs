using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.LocalSettings;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Patches.LocalSettings;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
public static class OptionsMenuPatches
{
    private static readonly Dictionary<int, List<GameObject>> TabButtons = [];
    private static int _currentPage = 1;

    internal static OptionsMenuBehaviour? Instance { get; private set; }

    [SuppressMessage(
        "Critical Code Smell",
        "S2223:Non-constant static fields should not be visible",
        Justification = "Internal behaviour that does not need property-level validation.")]
    [SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1401:Fields should be private",
        Justification = "Read above.")]
    internal static BoxCollider2D MaskCollider = null!;

    private static SpriteRenderer? background;

    /// <summary>
    /// Creates the <see cref="LocalSettingsTab"/>s and their content.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    public static void StartPostfix(OptionsMenuBehaviour __instance)
    {
        Instance = __instance;

        // Fix for tabs not being clickable in the main menu
        if (!AmongUsClient.Instance.IsInGame)
        {
            __instance.Background.GetComponent<BoxCollider2D>().enabled = false;
            __instance.transform.FindChild("Tint").SetLocalZ(5.6f);
        }

        background = __instance.Background;

        var maskObj = new GameObject
        {
            layer = 5,
            name = "SpriteMask",
        };
        maskObj.transform.SetParent(__instance.transform);
        maskObj.transform.localPosition = new Vector3(0, -0.3f, 0);
        maskObj.transform.localScale = new Vector3(500, 120, 1);

        var blank = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f));

        var mask = maskObj.AddComponent<SpriteMask>();
        mask.sprite = blank;
        mask.isCustomRangeActive = true;
        mask.sortingLayerName = "Default";
        mask.backSortingOrder = 100;
        mask.frontSortingOrder = 500;

        MaskCollider = maskObj.AddComponent<BoxCollider2D>();
        MaskCollider.size = new Vector2(0.01f, 0.1f);
        MaskCollider.isTrigger = true;
        MaskCollider.enabled = true;

        _currentPage = 1;
        TabButtons.Clear();
        float yOffset = 0;
        var i = 0;
        var tabIdx = 0;
        var page = 1;
        LocalSettingsTab.TabGroups.Clear();
        foreach (var settings in LocalSettingsManager.AvailableTabs)
        {
            var tab = settings.CreateTab(__instance);
            if (settings.ShouldCreateButton)
            {
                var button = settings.CreateTabButton(__instance, ref tabIdx, ref yOffset);
                button.GetComponent<TabGroup>().Content = tab;
                button.SetActive(page == _currentPage);

                if (!TabButtons.TryGetValue(page, out var list))
                {
                    TabButtons.Add(page, []);
                    list = TabButtons[page];
                }

                list.Add(button);
                yOffset += 0.6f;
                tabIdx++;
                i++;
            }

            if (i > 7)
            {
                i = 0;
                yOffset = 0;
                page++;
            }
        }

        // Create page buttons
        // TODO: In-game menu has no close button. I tried making one from scratch but failed. Add one later plz
        if (TabButtons.Count <= 1)
        {
            return;
        }

        CreatePageButton(__instance, 3.2f, false, 1);
        CreatePageButton(__instance, 2.9f, true, -1);
    }

    private static void CreatePageButton(OptionsMenuBehaviour menu, float x, bool flip, int direction)
    {
        var obj = new GameObject("PageButton") { layer = 5 };
        obj.transform.SetParent(menu.transform);
        obj.transform.localPosition = new Vector3(x, 2.6f, -10);
        obj.transform.localScale = new Vector3(0.25f, 0.25f, 1);

        var rend = obj.AddComponent<SpriteRenderer>();
        rend.sprite = MiraAssets.NextButton.LoadAsset();
        rend.flipX = flip;

        var col = obj.AddComponent<BoxCollider2D>();
        col.size = rend.sprite.bounds.size;

        var rollover = obj.AddComponent<ButtonRolloverHandler>();
        rollover.Target = rend;
        rollover.OverColor = Palette.AcceptedGreen;
        rollover.OutColor = Color.white;

        var button = obj.AddComponent<PassiveButton>();
        button.Colliders = new Il2CppReferenceArray<Collider2D>([col]);
        button.OnClick = new Button.ButtonClickedEvent();
        button.OnClick.AddListener(
            (UnityAction)(() =>
            {
                ChangePage(direction);
                UpdatePages();
            }));
        button.OnMouseOver = new UnityEvent();
        button.OnMouseOver.AddListener((UnityAction)(() => rollover.DoMouseOver()));
        button.OnMouseOut = new UnityEvent();
        button.OnMouseOut.AddListener((UnityAction)(() => rollover.DoMouseOut()));
    }

    private static void ChangePage(int increase)
    {
        _currentPage += increase;
        if (_currentPage > TabButtons.Count)
        {
            _currentPage = 1;
        }

        if (_currentPage < 1)
        {
            _currentPage = TabButtons.Count;
        }
    }

    /// <summary>
    /// Modifies how opening tabs is handled for the custom ones to work.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.OpenTabGroup))]
    public static bool OpenTabGroupPrefix(OptionsMenuBehaviour __instance, ref int index)
    {
        __instance.Tabs.ToList().ForEach(x => x.Close()); // Close all vanilla tabs
        LocalSettingsManager.AvailableTabs.ForEach(CustomClose); // Close all mod tabs

        // Tabs with index 10 and above are the mod settings tabs
        if (index >= 10)
        {
            CustomOpen(LocalSettingsManager.AvailableTabs[index - 10]);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Makes the custom tabs close when opening the options menu and resets pages.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Open))]
    public static void OpenPostfix()
    {
        LocalSettingsManager.AvailableTabs.ForEach(CustomClose);
        _currentPage = 1;
        UpdatePages();
    }

    private static void UpdatePages()
    {
        foreach (var pages in TabButtons)
        {
            pages.Value?
                .Where(x => x != null)
                .Do(x => x?.SetActive(pages.Key == _currentPage));
        }
    }

    private static void CustomOpen(LocalSettingsTab tab)
    {
        if (tab.TabButton?.Button)
            tab.TabButton!.Button.color = tab.TabAppearance.TabButtonHoverColor;

        if (tab.TabButton?.Rollover)
            tab.TabButton!.Rollover.OutColor = tab.TabAppearance.TabButtonHoverColor;

        if (tab.TabButton?.Content)
            tab.TabButton!.Content.SetActive(true);

        if (background != null)
            background.color = tab.TabAppearance.TabColor;

        tab.Open();
    }

    private static void CustomClose(LocalSettingsTab tab)
    {
        if (tab.TabButton?.Button)
            tab.TabButton!.Button.color = tab.TabAppearance.TabButtonColor;

        if (tab.TabButton?.Rollover)
            tab.TabButton!.Rollover.OutColor = tab.TabAppearance.TabButtonColor;

        if (tab.TabButton?.Content)
            tab.TabButton!.Content.SetActive(false);

        if (background != null)
            background.color = Color.white;
    }
}
