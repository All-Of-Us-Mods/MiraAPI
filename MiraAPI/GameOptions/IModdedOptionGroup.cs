using System;
using UnityEngine;

namespace MiraAPI.GameOptions;

public interface IModdedOptionGroup
{
    /// <summary>
    /// The name of the group. Visible in options menu.
    /// </summary>
    public string GroupName { get; }
    
    /// <summary>
    /// A function that returns whether the group should be visible or not.
    /// </summary>
    public Func<bool> GroupVisible => () => true;
    
    /// <summary>
    /// The group color. This is used to color the group in the options menu.
    /// </summary>
    public Color GroupColor => Color.clear;
    
    /// <summary>
    /// The role the group is associated with. This is used for the advanced role options menu.
    /// </summary>
    public Type AdvancedRole => null;
}