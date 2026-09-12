using MiraAPI.Patches.Stubs;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using CppCollections = Il2CppSystem.Collections.Generic;
using Object = UnityEngine.Object;

namespace MiraAPI.Hud;

/// <summary>
/// Paginable <see cref="CustomPhoneMenu"/> using the <see cref="ShapeshifterPanel"/> as a base.
/// </summary>
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
public abstract class CustomPaginableMenu : CustomPhoneMenu<CustomPaginableMenu.MenuEntry>
{
    /// <summary>
    /// Menu Entry used for when multiple pages are needed.
    /// </summary>
    /// <param name="Panel">The panel.</param>
    /// <param name="SortKey">The key by which the panels are sorted.</param>
    public record MenuEntry(ShapeshifterPanel Panel, string SortKey) : IMenuEntry;

    /// <summary>
    /// Gets the name of the menu.
    /// </summary>
    protected abstract string Name { get; }

    /// <summary>
    /// Gets the prefab used to generate the search field.
    /// </summary>
    protected abstract TextBoxTMP? PrefabTextbox { get; }

    /// <inheritdoc/>
    protected override float MenuDepth => -60f;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    protected int currentPage;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    private TextBoxTMP? searchTextbox;
    private string searchText = string.Empty;
    private TextMeshPro? noResultsText;

    private const int ItemsPerPage = 15;

    /// <summary>
    /// Creates a <typeparamref name="TMenu"/>.
    /// </summary>
    /// <typeparam name="TMenu">The type of <see cref="CustomPaginableMenu"/>.</typeparam>
    /// <param name="onMouseOut">Function that can optionally be run when the mouse is moved outside a menu panel.</param>
    /// <param name="onMouseOver">Function that can optionally be run when the mouse is moved over a menu panel.</param>
    /// <returns>New <typeparamref name="TMenu"/> object.</returns>
    protected static new TMenu Create<TMenu>(PanelButtonOnMouse? onMouseOut = null, PanelButtonOnMouse? onMouseOver = null) where TMenu : CustomPaginableMenu, new()
    {
        TMenu customMenu = CustomPhoneMenu.Create<TMenu>(onMouseOut, onMouseOver);

        var nextButton = Object.Instantiate(customMenu.backButton, customMenu.transform).gameObject;
        nextButton.transform.localPosition = new Vector3(1.85f, -2.185f, customMenu.MenuDepth);
        nextButton.transform.localScale = new Vector3(0.65f, 0.65f, 1);
        nextButton.name = "RightArrowButton";
        nextButton.GetComponent<SpriteRenderer>().sprite = MiraAssets.NextButton.LoadAsset();
        nextButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();

        var passiveButton = nextButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new Button.ButtonClickedEvent();
        passiveButton.OnClick.AddListener((UnityAction)customMenu.NextPage);

        var backButton = Object.Instantiate(nextButton, customMenu.transform).gameObject;
        backButton.transform.localPosition = new Vector3(-1.85f, -2.185f, customMenu.MenuDepth);
        backButton.name = "LeftArrowButton";
        backButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        backButton.GetComponent<SpriteRenderer>().flipX = true;
        var prevPassive = backButton.gameObject.GetComponent<PassiveButton>();
        prevPassive.OnClick.AddListener((UnityAction)customMenu.PreviousPage);

        customMenu.PhoneUI.GetChild(0).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;
        customMenu.PhoneUI.GetChild(1).GetComponent<SpriteRenderer>().material =
            PlayerControl.LocalPlayer.cosmetics.currentBodySprite.BodySprite.material;

        return customMenu;
    }

    private static string NormalizeForSearch(string? text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : text.Trim().ToLowerInvariant();
    }

    private List<MenuEntry> GetFilteredEntries()
    {
        var query = NormalizeForSearch(searchText);

        if (string.IsNullOrEmpty(query))
            return MenuEntries;

        return [.. MenuEntries
            .Where(e => e.SortKey.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.SortKey.Equals(query, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(e => e.SortKey.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(e => e.SortKey.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ThenBy(e => e.SortKey, StringComparer.OrdinalIgnoreCase)];
    }

    private static int GetTotalPages(int itemCount)
    {
        return Mathf.Max(1, Mathf.CeilToInt(itemCount / (float)ItemsPerPage));
    }

    private void RefreshControllerOverlay(CppCollections.List<UiElement> list)
    {
        if (ControllerManager.Instance && backButton != null)
        {
            ControllerManager.Instance.OpenOverlayMenu(name, backButton, defaultButtonSelected, list);
        }
    }

    private IEnumerator CoRestoreFocus()
    {
        yield return null;
        searchTextbox?.GiveFocus();
    }

    private void NextPage()
    {
        var filtered = GetFilteredEntries();
        var pages = GetTotalPages(filtered.Count);
        currentPage = (currentPage + 1) % pages;
        var list = ShowPage();
        RefreshControllerOverlay(list);
    }

    private void PreviousPage()
    {
        var filtered = GetFilteredEntries();
        var pages = GetTotalPages(filtered.Count);
        currentPage = (currentPage - 1 + pages) % pages;
        var list = ShowPage();
        RefreshControllerOverlay(list);
    }

    /// <summary>
    /// Shows the current page.
    /// </summary>
    /// <returns>The Il2CPP list of ui elements displayed on the page.</returns>
    public CppCollections.List<UiElement> ShowPage()
    {
        foreach (var entry in MenuEntries)
        {
            entry.Panel.gameObject.SetActive(false);
        }

        var filtered = GetFilteredEntries();
        noResultsText?.gameObject.SetActive(filtered.Count == 0 && !string.IsNullOrWhiteSpace(searchText));
        var totalPages = GetTotalPages(filtered.Count);
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        var list = filtered.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage).ToList();
        var list2 = new CppCollections.List<UiElement>();

        for (var i = 0; i < list.Count; i++)
        {
            var entry = list[i];
            var num = i % 3;
            var num2 = i / 3 % 5;
            entry.Panel.transform.localPosition =
                new Vector3(xStart + num * xOffset, yStart + num2 * yOffset, -1f);
            entry.Panel.gameObject.SetActive(true);
            list2.Add(entry.Panel.Button);
        }

        return list2;
    }

    /// <summary>
    /// Ensures that the search UI exists.
    /// </summary>
    protected void EnsureSearchUi()
    {
        if (searchTextbox != null)
        {
            return;
        }

        var gridCenterX = xStart + xOffset;
        var desiredSearchBarCenterY = yStart + 0.55f;

        if (PrefabTextbox == null)
        {
            return;
        }

        var searchRoot = PrefabTextbox.transform.parent != null ? PrefabTextbox.transform.parent.gameObject : PrefabTextbox.gameObject;
        var searchObj = Object.Instantiate(searchRoot, transform);
        searchObj.name = $"{Name}SearchBar";

        foreach (var aspect in searchObj.GetComponentsInChildren<AspectPosition>(true))
        {
            aspect.DestroyImmediate();
        }

        searchTextbox = searchObj.GetComponentInChildren<TextBoxTMP>(true);
        if (searchTextbox == null)
        {
            return;
        }

        try
        {
            var placeholder = searchTextbox.transform.parent.GetChild(2).GetComponent<TextMeshPro>();
            placeholder?.gameObject.SetActive(false);
        }
        catch
        {
            foreach (var tmp in searchObj.GetComponentsInChildren<TextMeshPro>(true))
            {
                if (tmp != searchTextbox.outputText &&
                    (tmp.text.Contains("Search", StringComparison.OrdinalIgnoreCase) ||
                     tmp.text.Contains("Here", StringComparison.OrdinalIgnoreCase)))
                {
                    tmp.gameObject.SetActive(false);
                }
            }
        }

        searchObj.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        searchObj.transform.localPosition = new Vector3(gridCenterX, desiredSearchBarCenterY, -1f);

        var searchBounds = CalcSpriteBoundsInParentSpace(transform, searchObj);
        var deltaX = gridCenterX - searchBounds.center.x;
        var deltaY = desiredSearchBarCenterY - searchBounds.center.y;
        searchObj.transform.localPosition += new Vector3(deltaX, deltaY, 0f);
        searchBounds = CalcSpriteBoundsInParentSpace(transform, searchObj);

        var wikiClickSound = HudManager.Instance?.MapButton?.ClickSound;

        var searchFocusButton = searchTextbox.gameObject.GetComponent<PassiveButton>()
                             ?? searchTextbox.gameObject.AddComponent<PassiveButton>();
        if (wikiClickSound != null)
        {
            searchFocusButton.ClickSound = wikiClickSound;
        }
        searchFocusButton.OnClick.RemoveAllListeners();
        searchFocusButton.OnClick.AddListener((UnityAction)(Action)(() =>
        {
            searchTextbox.GiveFocus();
        }));

        searchTextbox.SetText(string.Empty);
        searchTextbox.OnChange.RemoveAllListeners();
        searchTextbox.OnChange.AddListener((UnityAction)(Action)(() =>
        {
            searchText = searchTextbox.outputText.text ?? string.Empty;
            currentPage = 0;
            var list = ShowPage();
            RefreshControllerOverlay(list);

            if (searchTextbox != null)
            {
                Coroutines.Start(CoRestoreFocus());
            }
        }));

        var label = Object.Instantiate(HudManager.Instance?.TaskPanel.taskText, transform);
        if (label != null)
        {
            label.name = $"{Name}SearchLabel";
            label.text = "Search";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = label.fontSizeMin = label.fontSizeMax = 2.1f;
            label.color = Color.white;
            label.transform.localPosition = new Vector3(gridCenterX, searchBounds.max.y + 0.18f, -1f);

            if (searchTextbox.outputText != null)
            {
                label.font = searchTextbox.outputText.font;
                label.fontMaterial = searchTextbox.outputText.fontMaterial;
            }
        }

        noResultsText = Object.Instantiate(HudManager.Instance?.TaskPanel.taskText, transform);
        if (noResultsText != null)
        {
            noResultsText.name = $"{Name}NoResultsText";
            noResultsText.text = "No results";
            noResultsText.alignment = TextAlignmentOptions.Center;
            noResultsText.fontSize = noResultsText.fontSizeMin = noResultsText.fontSizeMax = 2.25f;
            noResultsText.color = Color.white;
            noResultsText.transform.localPosition = new Vector3(gridCenterX, yStart + 0.1f, -1f);
            noResultsText.gameObject.SetActive(false);
        }

        if (backButton == null || searchTextbox == null)
            return;

        var clearButtonX = searchBounds.max.x + 0.2f;
        var clearButtonY = searchBounds.center.y;

        var clearObj = Object.Instantiate(backButton.gameObject, transform);
        clearObj.name = "ClearSearchButton";
        clearObj.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
        clearObj.transform.localPosition = new Vector3(clearButtonX, clearButtonY, -1f);

        clearObj.GetComponent<CloseButtonConsoleBehaviour>()?.DestroyImmediate();
        clearObj.GetComponent<AspectPosition>()?.DestroyImmediate();

        var clearSearchButton = clearObj.GetComponent<PassiveButton>();
        if (clearSearchButton == null)
        {
            return;
        }
        if (wikiClickSound != null)
        {
            clearSearchButton.ClickSound = wikiClickSound;
        }

        clearSearchButton.OnClick.RemoveAllListeners();
        clearSearchButton.OnClick = new Button.ButtonClickedEvent();
        clearSearchButton.OnClick.AddListener((UnityAction)(() =>
        {
            if (searchTextbox == null)
            {
                return;
            }

            searchTextbox.SetText(string.Empty);
            searchText = string.Empty;
            currentPage = 0;
            var list = ShowPage();
            RefreshControllerOverlay(list);
        }));
    }

    private static Bounds CalcSpriteBoundsInParentSpace(Transform parent, GameObject root)
    {
        var first = true;
        var bounds = default(Bounds);

        foreach (var r in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (r == null || r.sprite == null)
                continue;

            var b = r.bounds;
            var c = b.center;
            var e = b.extents;

            var p1 = parent.InverseTransformPoint(new Vector3(c.x - e.x, c.y - e.y, c.z));
            var p2 = parent.InverseTransformPoint(new Vector3(c.x - e.x, c.y + e.y, c.z));
            var p3 = parent.InverseTransformPoint(new Vector3(c.x + e.x, c.y - e.y, c.z));
            var p4 = parent.InverseTransformPoint(new Vector3(c.x + e.x, c.y + e.y, c.z));

            if (first)
            {
                bounds = new Bounds(p1, Vector3.zero);
                first = false;
            }

            bounds.Encapsulate(p1);
            bounds.Encapsulate(p2);
            bounds.Encapsulate(p3);
            bounds.Encapsulate(p4);
        }

        return bounds;
    }

    /// <summary>
    /// Begins/opens the custom player menu. After registering panels, it will prepare the search, pages, and open the menu.
    /// </summary>
    /// <param name="registerEntryPanels">Function where all panels should be registered.</param>
    protected void Begin(Action registerEntryPanels)
    {
        MinigameStubs.Begin(Component, null);

        searchText = string.Empty;
        currentPage = 0;

        registerEntryPanels();

        EnsureSearchUi();

        var list2 = ShowPage();

        ControllerManager.Instance.OpenOverlayMenu(name, backButton, defaultButtonSelected, list2);
    }
}
