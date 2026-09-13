using AmongUs.GameOptions;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MiraAPI.Hud;

/// <summary>
/// Component registered in Il2Cpp domain to handle Unity events for custom menus.
/// </summary>
[RegisterInIl2Cpp]
public class CustomPhoneMenuComponent(IntPtr cppPtr) : Minigame(cppPtr)
{
    [SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity convention.")]
    private void OnDisable()
    {
        if (ControllerManager.Instance)
        {
            ControllerManager.Instance.CloseOverlayMenu(name);
        }
    }
}

/// <summary>
/// Defines an entry in a <see cref="CustomPhoneMenu"/> with a Panel.
/// </summary>
public interface IMenuEntry
{
    /// <summary>
    /// Gets the base Panel of the menu entry.
    /// </summary>
    ShapeshifterPanel Panel { get; }
}

/// <summary>
/// Defines a custom menu with a list of <see cref="ICustomMenu"/> entries.
/// </summary>
public interface ICustomMenu
{
    /// <summary>
    /// Gets all registered <see cref="IMenuEntry"/>s.
    /// </summary>
    List<IMenuEntry> MenuEntries { get; }
}

/// <summary>
/// Defines a custom menu with a list of only <typeparamref name="TMenu"/> entries.
/// <para/>
/// Only implement this if you want to define a single new type of menu entries explicitly.
/// Must reference the member of the <see cref="CustomPhoneMenu"/> superclass that is being hidden by this one to work.
/// </summary>
/// <typeparam name="TMenu">The type of menu entry.</typeparam>
public interface ICustomMenu<TMenu> : ICustomMenu where TMenu : IMenuEntry
{
    /// <summary>
    /// Gets all registered <typeparamref name="TMenu"/>s.
    /// </summary>
    new List<TMenu> MenuEntries { get; }
}

/// <summary>
/// Custom Phone Menu logic using the <see cref="ShapeshifterPanel"/> as a base.
/// </summary>
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Unity Convention.")]
[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Read above.")]
[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Read above.")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Proxy for Unity properties.")]
public abstract class CustomPhoneMenu : ICustomMenu
{
    /// <summary>
    /// Menu Entry used when specifically only the Panel itself is required.
    /// </summary>
    /// <param name="Panel">The <see cref="ShapeshifterPanel"/> instance.</param>
    protected sealed record class BasicEntry(ShapeshifterPanel Panel) : IMenuEntry
    {
        /// <summary>
        /// Implicitly converts to <see cref="ShapeshifterPanel"/>.
        /// </summary>
        /// <param name="entry">The entry that contains the reference to the panel.</param>
        public static implicit operator ShapeshifterPanel(BasicEntry entry)
        {
            return entry.Panel;
        }

        /// <summary>
        /// Implicitly converts to an instance of <see cref="BasicEntry"/>.
        /// </summary>
        /// <param name="panel">The panel instance to make an entry off of.</param>
        public static implicit operator BasicEntry(ShapeshifterPanel panel)
        {
            return new(panel);
        }
    }

    /// <summary>
    /// A delegate that is invoked when the mouse interacts with a panel.
    /// </summary>
    /// <param name="highlight">The panel's highlight that the delegate modifies.</param>
    /// <param name="icon">The panel's icon that the delegate modifies.</param>
    /// <param name="isSelected">A flag that indicates if the current panel is selected.</param>
    public delegate void PanelButtonOnMouse(SpriteRenderer highlight, SpriteRenderer icon, bool isSelected);

    /// <summary>
    /// Gets the wrapped component that the menu modifies.
    /// </summary>
    public CustomPhoneMenuComponent Component { get; internal set; } = null!;

    /// <inheritdoc/>
    public List<IMenuEntry> MenuEntries { get; protected set; } = [];

    /// <summary>
    /// Gets all of the panels of the menu.
    /// </summary>
    public List<ShapeshifterPanel> EntryPanels => [.. MenuEntries.Select(e => e.Panel)];

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member (Justification: Unity convention.)
#pragma warning disable S1104 // Fields should not have public accessibility (Justification: Read above.)
    public Transform transform => Component.transform;
    public GameObject gameObject => Component.gameObject;
    public string name => Component.name;

    public float xStart = -0.8f;
    public float yStart = 2.15f;
    public float xOffset = 1.95f;
    public float yOffset = -0.65f;

    public ShapeshifterPanel panelPrefab;
    public UiElement backButton;
    public UiElement defaultButtonSelected;

    protected PanelButtonOnMouse? onMouseOverAction;
    protected PanelButtonOnMouse? onMouseOutAction;
#pragma warning restore S1104 // Fields should not have public accessibility
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Gets the transform of the menu's Phone UI.
    /// </summary>
    public Transform PhoneUI => transform.FindChild("PhoneUI");

    /// <summary>
    /// Gets the z-axis depth that the menu is instantiated at.
    /// </summary>
    protected virtual float MenuDepth => -50f;

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public void Close()
    {
        Component.Close();
    }

    /// <summary>
    /// Creates a <typeparamref name="TMenu"/>.
    /// </summary>
    /// <typeparam name="TMenu">The type of <see cref="CustomPhoneMenu"/>.</typeparam>
    /// <param name="onMouseOut">Function that can optionally be run when the mouse is moved outside a menu panel.</param>
    /// <param name="onMouseOver">Function that can optionally be run when the mouse is moved over a menu panel.</param>
    /// <returns>New <typeparamref name="TMenu"/> object.</returns>
    protected static TMenu Create<TMenu>(PanelButtonOnMouse? onMouseOut = null, PanelButtonOnMouse? onMouseOver = null) where TMenu : CustomPhoneMenu, new()
    {
        var shapeShifterRole = RoleManager.Instance.GetRole(RoleTypes.Shapeshifter);

        var ogMenu = shapeShifterRole.TryCast<ShapeshifterRole>()!.ShapeshifterMenu;
        var newMenu = Object.Instantiate(ogMenu);
        var component = newMenu.gameObject.AddComponent<CustomPhoneMenuComponent>();
        var customMenu = new TMenu
        {
            Component = component,
            panelPrefab = newMenu.PanelPrefab,
            xStart = newMenu.XStart,
            yStart = newMenu.YStart,
            xOffset = newMenu.XOffset,
            yOffset = newMenu.YOffset,
            defaultButtonSelected = newMenu.DefaultButtonSelected,
            backButton = newMenu.BackButton,
        };

        var back = customMenu.backButton.GetComponent<PassiveButton>();
        back.OnClick.RemoveAllListeners();
        back.OnClick.AddListener((UnityAction)customMenu.Close);

        component.CloseSound = newMenu.CloseSound;
        component.logger = newMenu.logger;
        component.OpenSound = newMenu.OpenSound;

        newMenu.DestroyImmediate();

        customMenu.transform.SetParent(Camera.main!.transform, false);
        customMenu.transform.localPosition = new Vector3(0f, 0f, customMenu.MenuDepth);

        customMenu.onMouseOverAction = onMouseOver;
        customMenu.onMouseOutAction = onMouseOut;

        return customMenu;
    }

    /// <summary>
    /// Register new menu panels given a set of entries.
    /// </summary>
    /// <typeparam name="TEntry">The type of the entry.</typeparam>
    /// <param name="entries">The entries to create panels for.</param>
    /// <param name="entryPanelConfig">Function to configure the menu's panel once created, given its index and entry.</param>
    /// <param name="menuEntryMaker">Function to create a <see cref="IMenuEntry"/> for a given panel and it's entry.
    ///     If <see langword="null"/>, it will create <see cref="BasicEntry"/>s, acting as wrappers for the <see cref="ShapeshifterPanel"/>s.</param>
    protected void RegisterPanels<TEntry>(
        IEnumerable<TEntry> entries,
        Action<ShapeshifterPanel, int, TEntry> entryPanelConfig,
        Func<ShapeshifterPanel, TEntry, IMenuEntry>? menuEntryMaker = null)
    {
        int currentEntries = MenuEntries.Count;

        var list = entries.ToList();

        for (var i = 0; i < list.Count; i++)
        {
            var index = currentEntries + i;
            var entry = list[i];

            var shapeshifterPanel = Object.Instantiate(panelPrefab, transform);
            shapeshifterPanel.transform.localPosition = new Vector3(0f, 0f, -1f);
            entryPanelConfig(shapeshifterPanel, index, entry);

            menuEntryMaker ??= (s, _) => new BasicEntry(s);
            var menuEntry = menuEntryMaker(shapeshifterPanel, entry);
            MenuEntries.Add(menuEntry);

            var button = shapeshifterPanel.Button;
            var nameplate = shapeshifterPanel.gameObject.transform.FindChild("Nameplate");
            var highlight = nameplate.FindChild("Highlight").GetComponent<SpriteRenderer>();
            var icon = highlight.transform.GetChild(0).GetComponent<SpriteRenderer>();

            if (onMouseOverAction != null)
            {
                button.OnMouseOver.RemoveAllListeners();
                button.OnMouseOver = new UnityEvent();
                button.OnMouseOver.AddListener((UnityAction)
                    (() => onMouseOverAction(highlight, icon, IsEntrySelected(menuEntry))));
            }
            if (onMouseOutAction != null)
            {
                button.OnMouseOut.RemoveAllListeners();
                button.OnMouseOut = new UnityEvent();
                button.OnMouseOut.AddListener((UnityAction)
                    (() => onMouseOutAction(highlight, icon, IsEntrySelected(menuEntry))));
            }
        }
    }

    /// <inheritdoc cref="RegisterPanels{TEntry}(IEnumerable{TEntry}, Action{ShapeshifterPanel, int, TEntry}, Func{ShapeshifterPanel, TEntry, IMenuEntry}?)"/>
    /// <param name="entries"></param>
    /// <param name="onEntryClick">Action to perform when a given entry is clicked on.
    ///     Argument will be <see langword="null"/> if the back button was clicked instead.</param>
    /// <param name="entryPanelActionConfig">Function to configure the menu's panel once created, given its index, entry, and onClick action.</param>
    /// <param name="menuEntryMaker"></param>
    protected void RegisterPanels<TEntry>(
        IEnumerable<TEntry> entries,
        Action<TEntry?> onEntryClick,
        Action<ShapeshifterPanel, int, TEntry, Action> entryPanelActionConfig,
        Func<ShapeshifterPanel, TEntry, IMenuEntry>? menuEntryMaker = null)
    {
        RegisterPanels(entries, (p, i, e) => entryPanelActionConfig(p, i, e, () => onEntryClick(e)), menuEntryMaker);
    }

    /// <summary>
    /// Determines if a given entry is currently selected, not just hovered over.
    /// </summary>
    /// <param name="entry">The <see cref="IMenuEntry"/> to check.</param>
    /// <returns><see langword="true"/> if the <paramref name="entry"/> is selected, else <see langword="false"/>.</returns>
    protected virtual bool IsEntrySelected(IMenuEntry entry)
    {
        return false;
    }

    /// <summary>
    /// Set the icon, over color, and unselected color for a given entry.
    /// </summary>
    /// <param name="menuEntry">The menu entry.</param>
    /// <param name="sprite">The <see cref="Sprite"/> to use as the icon, if any.</param>
    /// <param name="overColor">The <see cref="Color"/> to use when hovering over an entry.</param>
    /// <param name="unselectedColor">The <see cref="Color"/> to use when not hovering over an entry.</param>
    protected static void SetNameplateAppearance(
        IMenuEntry menuEntry, LoadableAsset<Sprite>? sprite, Color? overColor, Color? unselectedColor)
    {
        ShapeshifterPanel panel = menuEntry.Panel;

        var nameplate = panel.gameObject.transform.FindChild("Nameplate");
        var highlight = nameplate.FindChild("Highlight").GetComponent<SpriteRenderer>();
        var icon = highlight.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            icon.sprite = sprite.LoadAsset();
        }
        var button = nameplate.GetComponent<ButtonRolloverHandler>();
        if (overColor is { } oColor)
        {
            button.OverColor = oColor;
        }
        if (unselectedColor is { } uColor)
        {
            button.UnselectedColor = uColor;
        }
    }
}

/// <inheritdoc cref="CustomPhoneMenu"/>
/// <typeparam name="TMenu">The type of menu entries.</typeparam>
public abstract class CustomPhoneMenu<TMenu> : CustomPhoneMenu, ICustomMenu<TMenu> where TMenu : IMenuEntry
{
    /// <inheritdoc/>
    public new List<TMenu> MenuEntries
    {
        get => [.. base.MenuEntries.Cast<TMenu>()];
        protected set => base.MenuEntries = [.. value.Cast<IMenuEntry>()];
    }
}
