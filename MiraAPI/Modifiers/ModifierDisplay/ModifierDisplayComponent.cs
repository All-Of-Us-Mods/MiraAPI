using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.Attributes;
using MiraAPI.LocalSettings;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Modifiers.ModifierDisplay;

/// <summary>
/// The code used to display <see cref="BaseModifier"/>s.
/// </summary>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity Convention.")]
public class ModifierDisplayComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets the instance of the <see cref="ModifierDisplayComponent"/>.
    /// </summary>
    public static ModifierDisplayComponent? Instance { get; private set; }

    private RectTransform _children = null!;
    private GameObject _modTemplate = null!;
    private PassiveButton _toggleButton = null!;
    private TextMeshPro _toggleBtnText = null!;

    private RectTransform _pagination = null!;
    private TextMeshPro _pageText = null!;
    private PassiveButton _nextButton = null!;
    private PassiveButton _backButton = null!;

    private int _currentPage;
    private const int ItemsPerPage = 3;

    private readonly Dictionary<BaseModifier, ModifierUiComponent> _modifiers = [];

    /// <summary>
    /// Gets a <see cref="ReadOnlyDictionary{TKey, TValue}"/> of the created <see cref="ModifierUiComponent"/>s.
    /// </summary>
    [HideFromIl2Cpp]
    public ReadOnlyDictionary<BaseModifier, ModifierUiComponent> Modifiers => new(_modifiers);

    /// <summary>
    /// Gets a value indicating whether the modifier display hud is currently open.
    /// </summary>
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        Instance = this;

        _children = transform.GetChild(0).GetComponent<RectTransform>();
        _modTemplate = transform.GetChild(2).gameObject;

        _toggleButton = transform.GetChild(1).GetComponent<PassiveButton>();
        _toggleButton.OnClick = new Button.ButtonClickedEvent();
        _toggleButton.OnClick.AddListener((UnityAction)(() =>
        {
            IsOpen = !IsOpen;
            _children.gameObject.SetActive(!_children.gameObject.activeSelf);
        }));

        _toggleButton.ClickSound = HudManager.Instance.GameMenu.StreamerModeButton.GetComponent<PassiveButton>().ClickSound;
        _toggleButton.HoverSound = HudManager.Instance.GameMenu.StreamerModeButton.GetComponent<PassiveButton>().HoverSound;
        _toggleButton.inactiveSprites = _toggleButton.transform.GetChild(1).gameObject;
        _toggleBtnText = _toggleButton.transform.GetChild(0).GetComponent<TextMeshPro>();

        _pagination = _children.transform.GetChild(0).GetComponent<RectTransform>();
        _pageText = _pagination.transform.GetChild(0).GetComponent<TextMeshPro>();
        _nextButton = _pagination.transform.GetChild(1).GetComponent<PassiveButton>();
        _backButton = _pagination.transform.GetChild(2).GetComponent<PassiveButton>();

        _toggleBtnText.font = _pageText.font = _nextButton.buttonText.font = _backButton.buttonText.font = HudManager.Instance.TaskPanel.taskText.font;
        _toggleBtnText.fontMaterial = _pageText.fontMaterial = _nextButton.buttonText.fontMaterial = _backButton.buttonText.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;

        _nextButton.OnClick = new Button.ButtonClickedEvent();
        _nextButton.OnClick.AddListener((UnityAction)(() =>
        {
            _currentPage++;
            UpdatePage();
        }));

        _backButton.OnClick = new Button.ButtonClickedEvent();
        _backButton.OnClick.AddListener((UnityAction)(() =>
        {
            _currentPage--;
            UpdatePage();
        }));

        IsOpen = false;
        _children.gameObject.SetActive(false);

        if (LocalSettingsTabSingleton<MiraApiSettings>.Instance.ModifiersHudLeftSide.Value)
        {
            var aspect = GetComponent<AspectPosition>();
            _toggleButton.transform.localPosition = new Vector3(-1f, 2.7f, 0f);
            _children.transform.localPosition = new Vector3(-0.2f, -0.05f, 0f);
            aspect.Alignment = AspectPosition.EdgeAlignments.LeftTop;
            aspect.DistanceFromEdge = new Vector3(1.8f, 2.55f, -20f);
            aspect.AdjustPosition();
        }
    }

    /// <summary>
    /// Use this to refresh the modifiers UI. Useful if you hide modifiers UIs based on options.
    /// </summary>
    public void RefreshModifiers()
    {
        UpdateModifiersList([.. PlayerControl.LocalPlayer.GetModifierComponent().ActiveModifiers]);
    }

    /// <summary>
    /// Use this to toggle the visibility of the modifiers tab.
    /// </summary>
    public void ToggleTab()
    {
        SetOpened(!IsOpen);
    }

    /// <summary>
    /// Use this to set the visibility of the modifiers tab.
    /// </summary>
    /// <param name="val">Whether it should be visible or not.</param>
    public void SetOpened(bool val)
    {
        IsOpen = val;
        _children.gameObject.SetActive(IsOpen);
        RefreshModifiers();
    }

    internal void DestroyComponent(ModifierUiComponent component)
    {
        _modifiers.Remove(component.Modifier!);
        component.gameObject.Destroy();
        RefreshModifiers();
    }

    [HideFromIl2Cpp]
    internal static ModifierDisplayComponent CreateDisplay()
    {
        var gameObject = Instantiate(MiraAssets.ModifierDisplay.LoadAsset(), HudManager.Instance.transform);
        var minigame = gameObject.AddComponent<ModifierDisplayComponent>();
        return minigame;
    }

    [HideFromIl2Cpp]
    private ModifierUiComponent CreateForModifier(BaseModifier modifier)
    {
        var newMod = Instantiate(_modTemplate, _children.transform);
        newMod.transform.name = modifier.ModifierName;
        var comp = newMod.AddComponent<ModifierUiComponent>();
        comp.Modifier = modifier;
        return comp;
    }

    private void UpdatePage()
    {
        var totalPages = Mathf.CeilToInt((float)_modifiers.Count / ItemsPerPage);
        _currentPage = Mathf.Clamp(_currentPage, 0, Mathf.Max(0, totalPages - 1));

        if (_modifiers.Count <= ItemsPerPage)
        {
            _modifiers.Do(x => x.Value.gameObject.SetActive(true));
            _pagination.gameObject.SetActive(false);
            return;
        }

        _pagination.gameObject.SetActive(true);
        if (_pageText != null)
        {
            _pageText.text = $"Page {_currentPage + 1}/{Mathf.Max(totalPages, 1)}";
        }

        _nextButton.gameObject.SetActive(true);
        _backButton.gameObject.SetActive(true);

        _modifiers.Do(x => x.Value.gameObject.SetActive(false));
        var start = _currentPage * ItemsPerPage;
        var end = Mathf.Min(start + ItemsPerPage, _modifiers.Count);
        var modifiers = _modifiers.ToArray();

        for (var i = start; i < end; i++)
        {
            modifiers[i].Value.gameObject.SetActive(true);
        }

        _pagination.SetAsLastSibling();
    }

    [HideFromIl2Cpp]
    internal void UpdateModifiersList(List<BaseModifier> modifiers)
    {
        var filteredModifiers = modifiers.Where(x => !x.HideOnUi && x.GetDescription() != string.Empty).ToList();
        var modifiersToAdd = filteredModifiers.ToList();

        foreach (var mod in _modifiers.ToArray())
        {
            if (modifiersToAdd.Contains(mod.Key))
            {
                modifiersToAdd.Remove(mod.Key);
                continue;
            }

            mod.Value.gameObject.Destroy();
            _modifiers.Remove(mod.Key);
        }

        _toggleBtnText.text = $"Modifiers ({filteredModifiers.Count})";

        _toggleButton.gameObject.SetActive(filteredModifiers.Count != 0);

        foreach (var mod in modifiersToAdd)
        {
            _modifiers.Add(mod, CreateForModifier(mod));
        }

        UpdatePage();
    }
}
