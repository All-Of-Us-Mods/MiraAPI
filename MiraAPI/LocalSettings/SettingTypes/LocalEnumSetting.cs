using System;
using System.Linq;
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
/// Local setting class for <see langword="enum"/>s.
/// </summary>
/// <param name="tab">The tab to create the setting in.</param>
/// <param name="configEntry">The config entry.</param>
/// <param name="enumType">The <see cref="Enum"/> type.</param>
/// <param name="name">The name of the setting.</param>
/// <param name="description">The description of the setting.</param>
/// <param name="values">The optional values array to replace the <see langword="enum"/> names.</param>
public class LocalEnumSetting(
    Type tab,
    ConfigEntryBase configEntry,
    Type enumType,
    string? name = null,
    string? description = null,
    string[]? values = null) : LocalSettingBase<int>(tab, configEntry, name, description)
{
    /// <summary>
    /// Gets the <see cref="Enum"/> type of the setting.
    /// </summary>
    public Type EnumType { get; } = enumType;

    /// <summary>
    /// Gets the <see langword="enum"/> values.
    /// </summary>
    public string[] Values { get; } = values ?? [.. Enum
        .GetValues(configEntry.SettingType)
        .Cast<Enum>()
        .Select(x => x.ToDisplayString())];

    private SpriteRenderer _highlight { get; set; }

    private TextMeshPro _btnText { get; set; }

    /// <inheritdoc />
    public override GameObject CreateOption(ToggleButtonBehaviour toggle, SlideBar slider, Transform parent, ref float offset, ref int order, bool last)
    {
        var button = Object.Instantiate(toggle, parent).GetComponent<PassiveButton>();
        var tmp = button.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        _btnText = tmp;
        var rollover = button.GetComponent<ButtonRolloverHandler>();
        tmp.GetComponent<TextTranslatorTMP>().Destroy();
        button.gameObject.SetActive(true);

        var toggleComp = button.GetComponent<ToggleButtonBehaviour>();
        var background = toggleComp.Background;
        var highlight = button.transform.FindChild("ButtonHighlight")?.GetComponent<SpriteRenderer>();
        if (highlight != null)
        {
            _highlight = highlight;
            highlight.color = Tab!.TabAppearance.EnumHoverColor;
            highlight.gameObject.SetActive(false);
        }
        toggleComp.Destroy();

        if (last && order == 1)
        {
            // Button in the middle
            button.transform.localPosition = new Vector3(0, 1.85f - offset, -7);
        }
        else
        {
            button.transform.localPosition = new Vector3(order == 1 ? -1.185f : 1.185f, 1.85f - offset, -7);
        }

        tmp.text = GetValueText();
        button.name = Name;
        button.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        rollover.OutColor = Tab!.TabAppearance.EnumColor;
        rollover.OverColor = Tab.TabAppearance.EnumHoverColor;
        background.color = Tab.TabAppearance.EnumColor;

        button.OnClick.AddListener((UnityAction)(() =>
        {
            int value = GetValue();
            value++;
            if (value >= Values.Length)
            {
                value = 0;
            }

            SetValue(value);
            tmp.text = GetValueText();
        }));
        button.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!Description.IsNullOrWhiteSpace())
            {
                tmp.text = Description;
            }
            highlight?.gameObject.SetActive(true);
        }));
        button.OnMouseOut.AddListener((UnityAction)(() =>
        {
            tmp.text = GetValueText();
            highlight?.gameObject.SetActive(false);
        }));

        Helpers.DivideSize(button.gameObject, 1.1f);

        order++;
        if (order > 2 && !last)
        {
            offset += 0.5f;
            order = 1;
        }
        if (last)
            offset += 0.6f;

        return button.gameObject;
    }

    /// <inheritdoc/>
    public override void RefreshOption()
    {
        _btnText.text = GetValueText();
        if (_highlight)
        {
            _highlight.gameObject.SetActive(false);
        }
    }

    /// <inheritdoc/>
    protected override string GetValueText()
    {
        return $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\">{Name.Translate()}: <b>{Values[GetValue()].Translate()}</font></b>";
    }
}
