using System;
using System.Collections.Generic;
using UnityEngine;

namespace MiraAPI.GameOptions;

public abstract class AbstractOptionGroup
{
    public List<IModdedOption> Options { get; } = [];
    
    /// <summary>
    /// The name of the group. Visible in options menu.
    /// </summary>
    public abstract string GroupName { get; }
    
    /// <summary>
    /// A function that returns whether the group should be visible or not.
    /// </summary>
    public virtual Func<bool> GroupVisible => () => true;
    
    /// <summary>
    /// The group color. This is used to color the group in the options menu.
    /// </summary>
    public virtual Color GroupColor => Color.clear;
    
    /// <summary>
    /// The role the group is associated with. This is used for the advanced role options menu.
    /// </summary>
    public virtual Type AdvancedRole => null;
}