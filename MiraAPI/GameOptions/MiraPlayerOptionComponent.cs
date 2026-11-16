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
public class MiraPlayerOptionComponent(IntPtr cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets or sets the <see cref="ModdedPlayerOption"/> associated with the object.
    /// </summary>
    [HideFromIl2Cpp]
    public ModdedPlayerOption PlayerOption { get; set; }
}
