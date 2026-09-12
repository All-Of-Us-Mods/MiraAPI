using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.Patches.LocalSettings;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace MiraAPI.LocalSettings;

/// <summary>
/// Represents a local settings tab. Gets a button by default.
/// </summary>
public abstract class LocalSettingsTab(ConfigFile config)
{
    /// <summary>
    /// Gets the name of the tab.
    /// </summary>
    public abstract string TabName { get; }

    /// <summary>
    /// Gets the tab's <see cref="LocalSettingTabAppearance"/>.
    /// </summary>
    public virtual LocalSettingTabAppearance TabAppearance { get; } = new();

    /// <summary>
    /// Gets a value indicating whether a button should be created for the tab.
    /// </summary>
    public virtual bool ShouldCreateButton => true;

    /// <summary>
    /// Gets a value indicating whether the tab should have category labels.
    /// </summary>
    protected virtual bool ShouldCreateLabels => true;

    /// <summary>
    /// Gets the mod's <see cref="ConfigFile"/>. Automatically found in the plugin loader.
    /// </summary>
    public ConfigFile Config { get; } = config ?? throw new ArgumentNullException(nameof(config));

    /// <summary>
    /// Gets the tab button instance.
    /// </summary>
    public TabGroup? TabButton { get; internal set; }

    /// <summary>
    /// Gets a collection of <see cref="TextMeshPro"/>s alongside the locale keys for them.
    /// </summary>
    public static Dictionary<TextMeshPro, string> TabGroups { get; } = [];

    /// <summary>
    /// Gets the list of <see cref="ILocalSetting"/>s.
    /// </summary>
    public List<ILocalSetting> Settings { get; } = [];

    /// <summary>
    /// Gets a collection of <see cref="TextMeshPro"/>s alongside the locale keys for them.
    /// </summary>
    public Dictionary<TextMeshPro, string> Labels { get; } = [];

    /// <summary>
    /// Gets the list of <see cref="LocalSettingsButton"/>s.
    /// </summary>
    public List<LocalSettingsButton> Buttons { get; } = [];

    /// <summary>
    /// Gets the index of this tab.
    /// </summary>
    protected int TabIndex => LocalSettingsManager.Tabs.IndexOf(this) + 10;

    private Scroller? Scroller { get; set; }

    /// <summary>
    /// Invoked when an option value is changed.
    /// </summary>
    /// <param name="configEntry">The config entry which was changed.</param>
    public virtual void OnOptionChanged(ConfigEntryBase configEntry)
    {
    }

    /// <summary>
    /// Refreshes setting names upon opening the tab.
    /// Override for custom behavior.
    /// </summary>
    public virtual void Open()
    {
        foreach (var setting in Settings)
        {
            setting.RefreshOption();
        }
        foreach (var button in Buttons)
        {
            button.RefreshButton();
        }

        foreach (var label in Labels)
        {
            label.Key.text =
                $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\"><b>{label.Value.Translate()}</b></font>";
        }
    }

    /// <summary>
    /// Creates the tab <see cref="GameObject"/> and it's content.
    /// </summary>
    /// <param name="instance">The <see cref="OptionsMenuBehaviour"/> instance.</param>
    /// <returns>The created tab <see cref="GameObject"/>.</returns>
    public virtual GameObject CreateTab(OptionsMenuBehaviour instance)
    {
        var tab = Object.Instantiate(instance.transform.FindChild("GeneralTab").gameObject, instance.transform);
        tab.name = $"{TabName}Tab";
        tab.transform.DestroyChildren();
        tab.gameObject.SetActive(false);

        if (Scroller == null)
        {
            Scroller = Helpers.CreateScroller(tab.transform, OptionsMenuPatches.MaskCollider);
        }

        if (TabButton != null)
        {
            TabButton.Content = tab.gameObject;
        }

        var generalLabel = instance.transform.FindChild("GeneralTab").FindChild("ControlGroup")
            .FindChild("ControlText_TMP").gameObject;
        var toggle = instance.transform.FindChild("GeneralTab").FindChild("ChatGroup").FindChild("CensorChatButton").GetComponent<ToggleButtonBehaviour>();
        var slider = instance.transform.FindChild("GeneralTab").FindChild("SoundGroup").FindChild("SFXSlider").GetComponent<SlideBar>();

        Dictionary<string, List<ILocalSetting>> entriesByGroup = [];
        foreach (var entry in Settings)
        {
            var group = entry.ConfigEntry.Definition.Section;
            if (!entriesByGroup.ContainsKey(group))
                entriesByGroup.Add(group, []);

            entriesByGroup[group].Add(entry);
        }

        float contentOffset = 0;
        var contentOrder = 1;
        var contentIndex = 1;

        foreach (var pair in entriesByGroup)
        {
            if (ShouldCreateLabels)
            {
                CreateLabel(generalLabel, Scroller.Inner, pair.Key, ref contentOffset);
                contentOrder = 1;
                contentIndex = 1;
            }

            foreach (var setting in pair.Value)
            {
                var obj = setting.CreateOption(
                    toggle,
                    slider,
                    Scroller.Inner,
                    ref contentOffset,
                    ref contentOrder,
                    contentIndex == pair.Value.Count);

                obj!.GetComponentsInChildren<SpriteRenderer>(true).Do(x =>
                {
                    x.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    x.sortingOrder = 150;
                    x.sortingLayerName = "Default";
                });

                obj.GetComponentsInChildren<MeshRenderer>(true).Do(x =>
                {
                    x.sortingOrder = 151;
                    x.sortingLayerName = "Default";
                });

                obj.GetComponentsInChildren<PassiveButton>().Do(x => { x.ClickMask = OptionsMenuPatches.MaskCollider; });

                contentIndex++;
            }

            contentOrder = 1;
        }

        contentIndex = 1;
        contentOrder = 1;
        foreach (var button in Buttons)
        {
            var obj = button.CreateButton(
                toggle,
                Scroller.Inner,
                ref contentOffset,
                ref contentOrder,
                contentIndex == Buttons.Count);

            obj.GetComponentsInChildren<SpriteRenderer>(true).Do(x =>
            {
                x.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                x.sortingOrder = 150;
                x.sortingLayerName = "Default";
            });

            obj.GetComponentsInChildren<MeshRenderer>(true).Do(x =>
            {
                x.sortingOrder = 151;
                x.sortingLayerName = "Default";
            });

            obj.GetComponentsInChildren<PassiveButton>().Do(x => { x.ClickMask = OptionsMenuPatches.MaskCollider; });

            contentIndex++;
        }

        Scroller.SetBounds(new FloatRange(0, (Settings.Count + Buttons.Count) * 0.5f - 5f), new FloatRange(0, 0));
        return tab;
    }

    /// <summary>
    /// Creates the tab button.
    /// </summary>
    /// <param name="instance">The <see cref="OptionsMenuBehaviour"/> instance.</param>
    /// <param name="tabIdx">The tab index.</param>
    /// <param name="offset">The current button offset.</param>
    /// <returns>The created button <see cref="GameObject"/>.</returns>
    public virtual GameObject CreateTabButton(OptionsMenuBehaviour instance, ref int tabIdx, ref float offset)
    {
        var tabButtonObject = Object.Instantiate(instance.Tabs[0], instance.transform);
        tabButtonObject.name = $"{TabName} Button";
        tabButtonObject.transform.localPosition = new Vector3(2.4f, 2.1f - offset, 5.5f);
        tabButtonObject.transform.localScale = new Vector3(1.25f, 1.25f, 1);
        tabButtonObject.Button.color = TabAppearance.TabButtonColor;
        TabButton = tabButtonObject;

        var tabButtonText = tabButtonObject.transform.FindChild("Text_TMP").GetComponent<TextMeshPro>();
        tabButtonText.GetComponent<TextTranslatorTMP>().Destroy(); // i hate text translators
        tabButtonText.transform.localPosition = new Vector3(0.078f, 0, 0);
        tabButtonText.transform.localScale = new Vector3(0.9f, 0.9f);
        tabButtonText.alignment = TextAlignmentOptions.Right;
        tabButtonText.text = $"<b>{GetShortName(TabName.Translate())}</b>";
        TabGroups.Add(tabButtonText, TabName);

        var tabButton = tabButtonObject.GetComponent<PassiveButton>();
        var rollover = tabButtonObject.Rollover;
        rollover.OverColor = TabAppearance.TabButtonHoverColor;
        rollover.OutColor = TabAppearance.TabButtonColor;

        SpriteRenderer? tabButtonRend = null;
        if (TabAppearance.TabIcon != null)
        {
            tabButtonRend = new GameObject("sprite").AddComponent<SpriteRenderer>();
            tabButtonRend.gameObject.layer = 5; // ui layer
            tabButtonRend.transform.SetParent(tabButtonObject.transform);
            tabButtonRend.transform.localPosition = new Vector3(0.5f, 0, -2);
            tabButtonRend.transform.localScale = new Vector3(0.2f, 0.2f, 1);
            tabButtonRend.sprite = TabAppearance.TabIcon.LoadAsset();
            tabButtonText.gameObject.SetActive(false);
        }

        var tabIndex = tabIdx; // Local copies of the tab variables so we can use them in the lambdas
        var tabOffset = offset;
        tabButton.OnClick.AddListener((UnityAction)(() => { instance.OpenTabGroup(tabIndex + 10); }));
        tabButton.OnMouseOver.AddListener((UnityAction)(() =>
        {
            tabButton.transform.localPosition = new Vector3(3.55f, 2.1f - tabOffset, 5.5f);
            tabButtonText.gameObject.SetActive(true);
            tabButtonText.transform.localPosition = new Vector3(-0.024f, 0, 0);
            tabButtonText.maxVisibleCharacters = int.MaxValue;
            tabButtonText.text = TabName.Translate();
            tabButtonText.alignment = TextAlignmentOptions.Left;
            tabButtonRend?.gameObject.SetActive(!TabAppearance.HideIconOnHover);
        }));
        tabButton.OnMouseOut.AddListener((UnityAction)(() =>
        {
            if (!tabButtonObject.Content.gameObject.activeSelf)
            {
                // tabButtonObject.Button.color = TabColor;
                // tabButtonObject.Rollover.OutColor = TabColor;
            }

            tabButton.transform.localPosition = new Vector3(2.4f, 2.1f - tabOffset, 5.5f);
            tabButtonText.alignment = TextAlignmentOptions.Right;
            if (tabButtonRend != null)
            {
                tabButtonRend.gameObject.SetActive(true);
                tabButtonText.gameObject.SetActive(false);
            }

            tabButtonText.transform.localPosition = new Vector3(0.1f, 0, 0);
            tabButtonText.maxVisibleCharacters = 4;
            tabButtonText.text = $"<b>{GetShortName(TabName.Translate())}</b>";
        }));

        return tabButtonObject.gameObject;
    }

    private void CreateLabel(GameObject template, Transform parent, string text, ref float offset)
    {
        var label = Object.Instantiate(template, parent);
        label.transform.localPosition = new Vector3(0, 1.85f - offset);
        label.GetComponent<TextTranslatorTMP>().Destroy();
        label.name = text;

        var tmp = label.GetComponent<TextMeshPro>();
        Labels.Add(tmp, text);
        tmp.text = $"<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Chat Message Masked\"><b>{text.Translate()}</b></font>";
        tmp.alignment = TextAlignmentOptions.Center;

        var meshRend = label.GetComponent<MeshRenderer>();
        meshRend.sortingLayerName = "Default";
        meshRend.sortingOrder = 151;

        offset += 0.5f;
    }

    internal static string GetShortName(string name)
    {
        var shortName = string.Empty;
        name.Split(' ').Do(x => shortName += x[0]);
        return shortName;
    }
}
