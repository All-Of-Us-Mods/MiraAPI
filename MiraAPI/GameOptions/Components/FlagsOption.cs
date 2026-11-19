using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Translation;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MiraAPI.GameOptions.Components;

// TODO: Add documentation once completed
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// An option behaviour that acts as a container for dynamically generated flag toggles.
/// </summary>
/// <param name="cppPtr">The pointer to the C++ object.</param>
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity convention.")]
[RegisterInIl2Cpp]
public class FlagsOption(nint cppPtr) : OptionBehaviour(cppPtr)
{
    public TextMeshPro TitleText;
    public TextMeshPro ValueText;
    public GameOptionButton Button;
    public bool Expanded;

    public int Value;
    public Il2CppStructArray<int> Flags;
    public Il2CppStructArray<StringNames> Values;
    public IFlagsOption Option;

    private int oldValue;

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
        var flagsGameSetting = data.TryCast<FlagsGameSetting>();

        if (flagsGameSetting == null)
        {
            return;
        }

        Title = flagsGameSetting.Title;
        Value = flagsGameSetting.Value;
        Flags = flagsGameSetting.Flags;
        Values = flagsGameSetting.Values;
    }

    public override void Initialize()
    {
        Value = Option.Value;
        ValueText.text = Option.GetDisplayValue();
        TitleText.text = TranslationController.Instance.GetString(Title);
        Button.AddOnClickListeners((Action)UpdateList);
        AdjustButtonState();
    }

    private void UpdateList()
    {

    }

    private void AdjustButtonState()
    {
        Button.ChangeButtonText(Expanded ? "⏶" : "⏷");
    }
}
