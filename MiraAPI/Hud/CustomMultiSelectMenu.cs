using MiraAPI.Patches.Stubs;
using MiraAPI.Utilities.Assets;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Hud;

/// <summary>
/// Multi-select <see cref="CustomPhoneMenu"/> using the <see cref="ShapeshifterPanel"/> as a base.
/// </summary>
/// <typeparam name="TEntry">The type of object each entry represents.</typeparam>
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity convention.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
public abstract class CustomMultiSelectMenu<TEntry> : CustomPhoneMenu<CustomMultiSelectMenu<TEntry>.MenuEntry> where TEntry : class
{
    private int totalSelections;
    private bool shouldConfirm;
    private bool canRepeat;
    private readonly List<MenuEntry> selectedEntries = [];

    private LoadableAsset<Sprite>? hoverSelectSprite;
    private LoadableAsset<Sprite>? hoverDeselectSprite;
    private Color? activeColor;
    private Color? hoverSelectColor;
    private Color? hoverDeselectColor;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "Unity convention.")]
    public UiElement confirmButton;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    private Action<List<TEntry>>? onSelection;

    /// <summary>
    /// Menu Entry used when multiple entries are at play.
    /// </summary>
    /// <param name="Panel">The panel of the entry.</param>
    /// <param name="Entry">The entry itself.</param>
    public record MenuEntry(ShapeshifterPanel Panel, TEntry Entry) : IMenuEntry;

    /// <summary>
    /// Creates a <typeparamref name="TMenu"/>.
    /// </summary>
    /// <typeparam name="TMenu">The type of <see cref="CustomMultiSelectMenu{TEntry}"/>.</typeparam>
    /// <param name="activeColor">The <see cref="Color"/> to use when an entry is selected but not hovered over.</param>
    /// <param name="hoverSelectSprite">The <see cref="Sprite"/> to use when an entry is hovered over while not selected.</param>
    /// <param name="hoverSelectColor">The <see cref="Color"/> to use when an entry is hovered over while not selected.</param>
    /// <param name="hoverDeselectSprite">The <see cref="Sprite"/> to use when an entry is hovered over while selected.</param>
    /// <param name="hoverDeselectColor">The <see cref="Color"/> to use when an entry is hovered over while selected.</param>
    /// <param name="onMouseOut">Function that can optionally be run when the mouse is moved outside a menu panel.</param>
    /// <param name="onMouseOver">Function that can optionally be run when the mouse is moved over a menu panel.</param>
    /// <returns>New <typeparamref name="TMenu"/> object.</returns>
    protected static TMenu Create<TMenu>(
        Color? activeColor = null,
        LoadableAsset<Sprite>? hoverSelectSprite = null,
        Color? hoverSelectColor = null,
        LoadableAsset<Sprite>? hoverDeselectSprite = null,
        Color? hoverDeselectColor = null,
        PanelButtonOnMouse? onMouseOut = null,
        PanelButtonOnMouse? onMouseOver = null
        ) where TMenu : CustomMultiSelectMenu<TEntry>, new()
    {
        TMenu customMenu = Create<TMenu>(onMouseOut, onMouseOver);

        customMenu.confirmButton = null!; // TODO: create/add confirm button

        var button = customMenu.confirmButton.GetComponent<PassiveButton>();
        button.OnClick.RemoveAllListeners();
        button.OnClick.AddListener((UnityAction)customMenu.OnCompleteSelection);

        customMenu.activeColor = activeColor;

        customMenu.hoverSelectSprite = hoverSelectSprite;
        customMenu.hoverSelectColor = hoverSelectColor;

        customMenu.hoverDeselectSprite = hoverDeselectSprite ?? hoverSelectSprite;
        customMenu.hoverDeselectColor = hoverDeselectColor ?? hoverSelectColor;

        return customMenu;
    }

    /// <summary>
    /// Setup the given <paramref name="panel"/> once created, given its index, entry, and onClick action.
    /// </summary>
    /// <param name="panel">The panel to configure.</param>
    /// <param name="i">The <paramref name="panel"/>'s index.</param>
    /// <param name="entry">The <paramref name="panel"/>'s <typeparamref name="TEntry"/> entry.</param>
    /// <param name="onClick">Action to perform when the <paramref name="panel"/> is clicked on.</param>
    protected abstract void SetupPanelEntry(ShapeshifterPanel panel, int i, TEntry entry, Action onClick);

    /// <summary>
    /// Begins/opens the custom multi-select menu.
    /// </summary>
    /// <param name="entries">All entries to give the custom menu.</param>
    /// <param name="onClick">Function called when all selections are made/confirmed.</param>
    /// <param name="totalSelections">The number of selections required.</param>
    /// <param name="shouldConfirm">Whether the entire selection should be confirmed manually.</param>
    /// <param name="canRepeat">If the same entry can be selected multiple times, else unselect entry on click.</param>
    protected void Begin(IEnumerable<TEntry> entries, Action<List<TEntry>?> onClick, int totalSelections, bool shouldConfirm, bool canRepeat = false)
    {
        MinigameStubs.Begin(Component, null);

        this.totalSelections = totalSelections;
        this.shouldConfirm = shouldConfirm;
        this.canRepeat = canRepeat;
        this.onSelection = onClick;

        var back = backButton.GetComponent<PassiveButton>();
        back.OnClick.AddListener((UnityAction)(() =>
        {
            selectedEntries.Clear();
            onClick(null);
        }));

        DebugAnalytics.Instance.Analytics.MinigameOpened(PlayerControl.LocalPlayer.Data, Component.TaskType);
        var list2 = new Il2CppSystem.Collections.Generic.List<UiElement>();
        RegisterPanels(
            entries,
            (shapeshifterPanel, i, entry) =>
            {
                var num = i % 3;
                var num2 = i / 3;
                shapeshifterPanel.transform.localPosition = new Vector3(xStart + num * xOffset, yStart + num2 * yOffset, -1f);
                SetupPanelEntry(shapeshifterPanel, i, entry, () => OnEntryClick(entry));
                list2.Add(shapeshifterPanel.Button);
            },
            (p, e) => new MenuEntry(p, e));
        ControllerManager.Instance.OpenOverlayMenu(name, backButton, defaultButtonSelected, list2);
    }

    /// <summary>
    /// Begins/opens the custom menu, only requiring a single selection.
    /// </summary>
    /// <param name="entries">All entries to give the custom menu.</param>
    /// <param name="onClick">Function called when the selection is made.</param>
    /// <param name="shouldConfirm">Whether the set of both selections should be confirmed manually.</param>
    protected void Begin(IEnumerable<TEntry> entries, Action<TEntry?> onClick, bool shouldConfirm)
    {
        Begin(entries, list => onClick(list?[0]), 1, shouldConfirm);
    }

    /// <summary>
    /// Begins/opens the custom menu, requiring two selections.
    /// </summary>
    /// <param name="entries">All entries to give the custom menu.</param>
    /// <param name="onClick">Function called when both selections are made.</param>
    /// <param name="shouldConfirm">Whether the set of both selections should be confirmed manually.</param>
    /// <param name="canRepeat">If the same entry can be selected both times, else unselect entry on click.</param>
    protected void Begin(IEnumerable<TEntry> entries, Action<TEntry?, TEntry?> onClick, bool shouldConfirm, bool canRepeat = false)
    {
        Begin(entries, list => onClick(list?[0], list?[1]), 2, shouldConfirm, canRepeat);
    }

    private void OnEntryClick(TEntry entry)
    {
        MenuEntry menuEntry = MenuEntries.First(e => e.Entry == entry);

        if (!canRepeat && selectedEntries.Remove(menuEntry)) // Unselect previous choice
        {
            // TODO: if confirm button is enabled, disable it
            SetNameplateAppearance(menuEntry, false);
            return;
        }

        if (selectedEntries.Count >= totalSelections) // Do not go past total selections.
        {
            return;
        }

        selectedEntries.Add(menuEntry);
        if (selectedEntries.Count <= totalSelections) // Add choice to list of selections
        {
            SetNameplateAppearance(menuEntry, true);
        }
        if (selectedEntries.Count < totalSelections)
        {
            return;
        }

        // Total selections reached
        if (shouldConfirm)
        {
            // TODO: enable confirm button
            return;
        }

        OnCompleteSelection();
        selectedEntries.Clear();
    }

    private void OnCompleteSelection()
    {
        onSelection!([.. selectedEntries.Select(se => se.Entry)]);
    }

    /// <inheritdoc/>
    protected override bool IsEntrySelected(IMenuEntry entry)
    {
        return selectedEntries.Any(e => e.Panel == entry.Panel);
    }

    /// <inheritdoc cref="CustomPhoneMenu.SetNameplateAppearance(IMenuEntry, LoadableAsset{Sprite}?, Color?, Color?)"/>
    /// <param name="menuEntry"></param>
    /// <param name="isSelected">Whether the menu entry is actively selected.</param>
    protected void SetNameplateAppearance(IMenuEntry menuEntry, bool isSelected)
    {
        SetNameplateAppearance(
            menuEntry,
            isSelected ? hoverDeselectSprite : hoverSelectSprite,
            isSelected ? hoverDeselectColor : hoverSelectColor,
            isSelected ? activeColor : Color.clear);
    }
}
