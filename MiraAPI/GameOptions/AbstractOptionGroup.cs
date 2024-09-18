using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiraAPI.GameOptions;

/// <summary>
/// Base class for option groups. An option group is a collection of options that are displayed together in the options menu.
/// </summary>
public abstract class AbstractOptionGroup
{
    internal List<IModdedOption> Options { get; } = [];

    /// <summary>
    /// Gets the name of the group. Visible in options menu.
    /// </summary>
    public abstract string GroupName { get; }

    /// <summary>
    /// Gets the function that determines whether the group should be visible or not.
    /// </summary>
    public virtual Func<bool> GroupVisible => () => true;

    /// <summary>
    /// Gets the group color. This is used to color the group in the options menu.
    /// </summary>
    public virtual Color GroupColor => Color.clear;

    /// <summary>
    /// Gets the group priority. This is used to determine the order in which groups are displayed in the options menu.
    /// Zero is the highest priority, and the default value is the max uint value.
    /// </summary>
    public virtual uint GroupPriority => uint.MaxValue;

    /// <summary>
    /// Gets the role the group is associated with. This is used for the advanced role options menu.
    /// </summary>
    public virtual Type? AdvancedRole => null;

    internal bool AllOptionsHidden { get; set; }

    internal CategoryHeaderMasked? Header { get; set; }
}
