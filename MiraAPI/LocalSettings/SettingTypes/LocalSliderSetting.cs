using System;
using BepInEx.Configuration;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MiraAPI.LocalSettings.SettingTypes;

/// <summary>
/// Local setting class for sliders.
/// </summary>
/// <param name="tab">The tab to create the setting in.</param>
/// <param name="configEntry">The config entry.</param>
/// <param name="name">The name of the setting.</param>
/// <param name="description">The description of the setting.</param>
/// <param name="sliderRange">The value range.</param>
/// <param name="suffixType">The suffix used for formatting.</param>
/// <param name="formatString">The format string used for formatting.</param>
/// <param name="roundValue">Should the value be rounded.</param>
/// <param name="displayValue">Should display the value next to the name.</param>
public class LocalSliderSetting(
    Type tab,
    ConfigEntryBase configEntry,
    string? name = null,
    string? description = null,
    FloatRange? sliderRange = null,
    bool displayValue = false,
    MiraNumberSuffixes? suffixType = null,
    string? formatString = null,
    bool roundValue = false) : LocalSettingBase<float>(tab, configEntry, name, description)
{
    /// <summary>
    /// Gets the range of the slider.
    /// </summary>
    public FloatRange SliderRange { get; } = sliderRange ?? new FloatRange(0, 100);

    /// <summary>
    /// Gets a value indicating whether the value should be displayed next to name.
    /// </summary>
    public bool DisplayValue { get; } = displayValue;

    /// <summary>
    /// Gets a format for the text to use to format the number.
    /// </summary>
    public string FormatString { get; } = formatString ?? "0.0";

    /// <summary>
    /// Gets a value indicating whether the value should be rounded.
    /// </summary>
    public bool RoundValue { get; } = roundValue;

    /// <summary>
    /// Gets the suffix for the number value.
    /// </summary>
    public MiraNumberSuffixes SuffixType { get; } = suffixType ?? MiraNumberSuffixes.None;

    private SlideBar _slider;

    /// <inheritdoc />
    public override GameObject CreateOption(ToggleButtonBehaviour toggle, SlideBar slider, Transform parent, ref float offset, ref int order, bool last)
    {
        var newSlider = Object.Instantiate(slider, parent).GetComponent<SlideBar>();
        _slider = newSlider;
        var rollover = newSlider.GetComponent<ButtonRolloverHandler>();
        newSlider.Title = newSlider.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>(); // Why the hell slider has a title property that is not even assigned???
        newSlider.Title.GetComponent<TextTranslatorTMP>().Destroy();
        newSlider.gameObject.SetActive(true);
        newSlider.Bar.color = Tab!.TabAppearance.SliderColor;

        if (order == 2)
            offset += 0.5f;

        newSlider.Bar.transform.localPosition = new Vector3(2.85f, 0, 0);
        newSlider.transform.localPosition = new Vector3(-2.12f, 1.85f - offset, -7);
        newSlider.name = Name;
        newSlider.Range = new FloatRange(-1.5f, 1.5f);
        newSlider.SetValue(Mathf.InverseLerp(SliderRange.min, SliderRange.max, GetValue()));
        rollover.OutColor = Tab.TabAppearance.SliderColor;
        rollover.OverColor = Tab.TabAppearance.SliderHoverColor;
        newSlider.Title.transform.localPosition = new Vector3(0.5f, 0, -1f);
        newSlider.Title.horizontalAlignment = DisplayValue ? HorizontalAlignmentOptions.Left : HorizontalAlignmentOptions.Center;
        newSlider.Title.text = GetValueText();

        newSlider.OnValueChange.AddListener((UnityAction)(() =>
        {
            SetValue(RoundValue
                ? Mathf.Round(Mathf.Lerp(SliderRange.min, SliderRange.max, newSlider.Value))
                : Mathf.Lerp(SliderRange.min, SliderRange.max, newSlider.Value));

            newSlider.Title.text = GetValueText();
        }));

        order = 1;
        offset += 0.5f;
        return newSlider.gameObject;
    }

    /// <inheritdoc/>
    public override void RefreshOption()
    {
        _slider.Title.text = GetValueText();
    }

    /// <inheritdoc/>
    protected override string GetValueText()
    {
        if (DisplayValue)
        {
            var value = GetValue();
            var formatted = Helpers.FormatValue(value, SuffixType, FormatString);
            var maxFormatted = Helpers.FormatValue(SliderRange.max, SuffixType, FormatString);
            return $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{Name.Translate()}: <b>{formatted} / {maxFormatted}</font></b>";
        }

        return $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{Name.Translate()}</font></b>";
    }
}
