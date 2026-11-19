using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Translation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiraAPI.GameOptions.Components;

/// <summary>
/// An option behaviour that acts as a container for dynamically generated flag toggles.
/// </summary>
/// <param name="cppPtr">The pointer to the C++ object.</param>
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity convention.")]
public class FlagsOption(nint cppPtr) : OptionBehaviour(cppPtr)
{
    public TextMeshPro TitleText;
    public TextMeshPro ValueText;
    public int Value;
    public Il2CppStructArray<StringNames> Values;

    private int oldValue = -1;

    private void Start()
    {
        Initialize();
    }

    public override int GetInt()
    {
        return Value;
    }

    public override void SetUpFromData(BaseGameSetting data, int maskLayer)
    {
        base.SetUpFromData(data, maskLayer);
        var stringGameSetting = data.TryCast<FlagsGameSetting>();

        if (stringGameSetting == null)
        {
            return;
        }

        Title = stringGameSetting.Title;
        Value = stringGameSetting.Value;
        Values = stringGameSetting.Values;
    }
}
