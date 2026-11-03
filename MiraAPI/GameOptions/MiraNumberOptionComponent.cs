using System;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using Reactor.Utilities.Attributes;
using UnityEngine;

namespace MiraAPI.GameOptions;

/// <summary>
/// Component added onto options generated via MiraAPI. This allows developers to hook patches onto options in general, without using a list.
/// </summary>
/// <param name="cppPtr">The pointer to the C++ object.</param>
[RegisterInIl2Cpp]
public class MiraNumberOptionComponent(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets or sets the <see cref="ModdedNumberOption"/> associated with the object.
    /// </summary>
    [HideFromIl2Cpp]
    public ModdedNumberOption NumberOption { get; set; }

    /// <summary>
    /// Gets or sets the default increment.
    /// </summary>
    public float DefaultIncrement { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether holding shift will split the increment in half.
    /// </summary>
    public bool ShiftIncrementToggle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating what Zero displays.
    /// </summary>
    public string ZeroValue { get; set; } = "#";

    /// <summary>
    /// Gets or sets a value indicating what Negative One displays.
    /// </summary>
    public string NegativeValue { get; set; } = "#";
}
