using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.GameOptions;

/// <summary>
/// Base class for option groups. An option group is a collection of options that are displayed together in the options menu.
/// </summary>
public abstract class AbstractOptionGroup
{
    internal List<IModdedOption> Options { get; } = [];

    /// <summary>
    /// Gets a list of the options which are a part of this group.
    /// </summary>
    public ReadOnlyCollection<IModdedOption> Children => new(Options);

    /// <summary>
    /// Gets the name of the group. Visible in options menu.
    /// </summary>
    public abstract string GroupName { get; }

    /// <summary>
    /// Gets the Optionable type of the group.
    /// </summary>
    public virtual Type? OptionableType => null;

    /// <summary>
    /// Gets a value indicating whether the group should be shown in the modifiers menu.
    /// </summary>
    // Completed: make this not a boolean
    [Obsolete("Use ParentMenu instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Retained.")]
    public virtual bool ShowInModifiersMenu => false;

    /// <summary>
    /// Gets a value indicating which menu the group is in.
    /// </summary>
    public virtual MenuCategory ParentMenu => MenuCategory.Game;

    /// <summary>
    /// Gets the function that determines whether the group should be visible or not.
    /// </summary>
    public virtual Func<bool> GroupVisible => () => true;

    /// <summary>
    /// Gets the group <see cref="Color"/>. This is used to color the group in the options menu.
    /// </summary>
    public virtual Color GroupColor => MiraApiPlugin.DefaultHeaderColor;

    /// <summary>
    /// Gets the group priority. This is used to determine the order in which groups are displayed in the options menu.
    /// Zero is the highest priority, and the default value is the max <see langword="uint"/> value.
    /// </summary>
    public virtual uint GroupPriority => uint.MaxValue;

    /// <summary>
    /// Gets the default notification settings for the group.
    /// </summary>
    public virtual OptionNotifConfiguration Configuration => new(new Color(0.7333f, 0.7333f, 0.7333f, 1));

    internal bool AllOptionsHidden { get; set; }

    internal CategoryHeaderMasked? Header { get; set; }

    internal bool Ready { get; set; }
}

/// <summary>
/// Base class for option groups. An option group is a collection of options that are displayed together in the options menu.
/// </summary>
/// <typeparam name="T">The type of the optionable that this group contains.</typeparam>
public abstract class AbstractOptionGroup<T> : AbstractOptionGroup where T : IOptionable
{
    /// <inheritdoc />
    public override Type OptionableType => typeof(T);

    /// <inheritdoc />
    public override MenuCategory ParentMenu
    {
        get
        {
            var type = typeof(T);
            if (typeof(BaseModifier).IsAssignableFrom(type))
            {
                return MenuCategory.Modifiers;
            }
            else if (typeof(RoleBehaviour).IsAssignableFrom(type))
            {
                return MenuCategory.Roles;
            }
            return MenuCategory.Game;
        }
    }
}

/// <summary>
/// Base class for option groups. An option group is a collection of options that are displayed together in the options menu.
/// </summary>
/// <typeparam name="T">The custom role that the group is for.</typeparam>
public abstract class AbstractRoleOptionGroup<T> : AbstractOptionGroup<T> where T : ICustomRole
{
    /// <inheritdoc />
    public override Type OptionableType => typeof(T);

    /// <inheritdoc />
    public override MenuCategory ParentMenu => MenuCategory.Roles;

    /// <inheritdoc />
    public override OptionNotifConfiguration Configuration
    {
        get
        {
            var role = CustomRoleManager.CustomMiraRoles.FirstOrDefault(x => x.GetType() == OptionableType);
            return role == null
                ? new(new Color(0.7333f, 0.7333f, 0.7333f, 1))
                : new(role.RoleColor, role.Configuration.IconTmp);
        }
    }
}

/// <summary>
/// Menu categories for option groups.
/// </summary>
public enum MenuCategory
{
    /// <summary>
    /// Placeholder for indexing purposes. Options don't exist in the preset tab.
    /// </summary>
    Preset,

    /// <summary>
    /// Determines the option group is in the game settings tab.
    /// </summary>
    Game,

    /// <summary>
    /// Determines the option group is in the role settings tab.
    /// </summary>
    Roles,

    /// <summary>
    /// Determines the option group is in the modifier settings tab.
    /// </summary>
    Modifiers,

    /// <summary>
    /// Determines the option group is in the first custom settings tab.
    /// </summary>
    CustomOne,

    /// <summary>
    /// Determines the option group is in the second custom settings tab.
    /// </summary>
    CustomTwo,
}
