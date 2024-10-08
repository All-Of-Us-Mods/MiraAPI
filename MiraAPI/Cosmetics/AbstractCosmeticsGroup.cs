using System;
using System.Collections.Generic;

namespace MiraAPI.Cosmetics;
public abstract class AbstractCosmeticsGroup
{
    public List<HatData> Hats { get; } = [];
    public List<SkinData> Skins { get; } = [];
    public List<VisorData> Visors { get; } = [];
    public List<NamePlateData> Nameplates { get; } = [];

    /// <summary>
    /// Gets the name of the group. Visible in options menu.
    /// </summary>
    public abstract string GroupName { get; }

    /// <summary>
    /// Gets a function that determines whether the group should be visible or not.
    /// </summary>
    public virtual Func<bool> GroupVisible => () => true;

    /// <summary>
    /// Gets the sorting mode of individual hats within this group.
    /// </summary>
    public virtual SortingMode SortingMode => SortingMode.AlphabeticName;

    /// <summary>
    /// Gets the priority of a group for sorting groups.
    /// </summary>
    public virtual int SortingPriority { get; }
}

public enum SortingMode
{
    Priority,
    AlphabeticProductId,
    AlphabeticStoreName,
    AlphabeticName,
}
