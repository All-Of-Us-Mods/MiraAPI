using System;
using BepInEx.Configuration;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MiraAPI.LocalSettings.SettingTypes;

/// <summary>
/// Local setting class for toggles.
/// </summary>
/// <param name="tab">The tab to create the setting in.</param>
/// <param name="configEntry">The config entry.</param>
/// <param name="name">The name of the setting.</param>
/// <param name="description">The description of the setting.</param>
public class LocalToggleSetting(
    Type tab,
    ConfigEntryBase configEntry,
    string? name = null,
    string? description = null
) : LocalSettingBase<bool>(tab, configEntry, name, description)
{
    private ToggleButtonBehaviour _toggle;

    /// <inheritdoc />
    public override GameObject CreateOption(ToggleButtonBehaviour toggle, SlideBar slider, Transform parent, ref float offset, ref int order, bool last)
    {
        var toggleObject = Object.Instantiate(toggle, parent).GetComponent<ToggleButtonBehaviour>();
        _toggle = toggleObject;
        var tmp = toggleObject.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        var passiveButton = toggleObject.GetComponent<PassiveButton>();
        var rollover = toggleObject.GetComponent<ButtonRolloverHandler>();
        toggleObject.gameObject.SetActive(true);

        if (last && order == 1)
        {
            // Toggle in the middle
            toggleObject.transform.localPosition = new Vector3(0, 1.85f - offset, -7);
        }
        else
        {
            toggleObject.transform.localPosition = new Vector3(order == 1 ? -1.185f : 1.185f, 1.85f - offset, -7);
        }

        toggleObject.BaseText = MiraLocaleManager.GetOrCreateLocaleString(Name);
        toggleObject.UpdateText(GetValue());
        toggleObject.name = Name.Translate();
        toggleObject.Background.color = GetValue() ? Tab!.TabAppearance.ToggleActiveColor : Tab!.TabAppearance.ToggleInactiveColor;
        passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();
        rollover.OverColor = Tab.TabAppearance.ToggleHoverColor;

        passiveButton.OnClick.AddListener((UnityAction)(() =>
        {
            SetValue(!GetValue());
            toggleObject.UpdateText(GetValue());
            toggleObject.Background.color = GetValue() ? Tab.TabAppearance.ToggleActiveColor : Tab.TabAppearance.ToggleInactiveColor;
        }));
        passiveButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            if (!Description.IsNullOrWhiteSpace())
            {
                tmp.text = Description;
            }
        }));
        passiveButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            toggleObject.UpdateText(GetValue());
            toggleObject.Background.color = GetValue() ? Tab.TabAppearance.ToggleActiveColor : Tab.TabAppearance.ToggleInactiveColor;
        }));

        Helpers.DivideSize(toggleObject.gameObject, 1.1f);

        order++;
        if (order > 2 && !last)
        {
            offset += 0.5f;
            order = 1;
        }
        if (last)
            offset += 0.6f;

        return toggleObject.gameObject;
    }

    /// <inheritdoc/>
    public override void RefreshOption()
    {
        _toggle.UpdateText(GetValue());
        _toggle.Background.color = GetValue() ? Tab!.TabAppearance.ToggleActiveColor : Tab!.TabAppearance.ToggleInactiveColor;
    }
}
