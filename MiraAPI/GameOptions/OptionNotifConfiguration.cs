using System;
using System.Diagnostics.CodeAnalysis;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;

namespace MiraAPI.GameOptions;

/// <summary>
/// Used to configure the specific visuals for option notifications.
/// </summary>
public record struct OptionNotifConfiguration
{
    [Obsolete("Default constructor is not supported. Please use the constructor that takes an AbstractOptionGroup parameter.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed", Justification = "Will not be removed, as this throws instead.")]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public OptionNotifConfiguration()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        throw new NotImplementedException("Default constructor is not supported. Please use the constructor that takes an AbstractOptionGroup parameter.");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionNotifConfiguration"/> struct based on a group.
    /// </summary>
    /// <param name="group">The <see cref="AbstractOptionGroup"/> in which you are configuring.</param>
    public OptionNotifConfiguration(AbstractOptionGroup group)
    {
        PopUpIconTmp = group.Configuration.PopUpIconTmp;
        PopUpTextColor = group.Configuration.PopUpTextColor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionNotifConfiguration"/> struct from scratch.
    /// </summary>
    /// <param name="color">The text <see cref="Color"/> for the configuration.</param>
    /// <param name="asset">The <see cref="TMP_SpriteAsset"/> icon for the configuration.</param>
    public OptionNotifConfiguration(Color color, TMP_SpriteAsset asset = null!)
    {
        PopUpIconTmp = asset;
        PopUpTextColor = color;
    }

    /// <summary>
    /// Gets or sets the <see cref="TMP_SpriteAsset"/> for the icon on the options pop-up.
    /// </summary>
    public TMP_SpriteAsset PopUpIconTmp { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Color"/> for the text on the options pop-up.
    /// </summary>
    [HideFromIl2Cpp]
    public Color PopUpTextColor { get; set; }
}
