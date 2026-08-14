using System.Diagnostics.CodeAnalysis;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using TMPro;
using UnityEngine;

namespace MiraAPI.Modifiers.ModifierDisplay;

/// <summary>
/// The code placed on every <see cref="BaseModifier"/> HUD object. Used to handle updating.
/// </summary>
[RegisterInIl2Cpp]
[SuppressMessage("Style", "IDE0051:Remove unused private members", Justification = "Unity Convention.")]
public class ModifierUiComponent(nint cppPtr) : MonoBehaviour(cppPtr)
{
    /// <summary>
    /// Gets the <see cref="BaseModifier"/> which this component is for.
    /// </summary>
    [HideFromIl2Cpp]
    public BaseModifier? Modifier { get; internal set; }

    // ReSharper disable InconsistentNaming
    private SpriteRenderer modBg = null!;
    private TextMeshPro desc = null!;
    private SpriteRenderer icon = null!;
    private TextMeshPro nameText = null!;
    private RectTransform descRect = null!;
    // ReSharper restore InconsistentNaming
    private void Awake()
    {
        if (Modifier == null) return;

        modBg = transform.GetChild(0).GetComponent<SpriteRenderer>();
        desc = modBg.transform.GetChild(0).GetComponent<TextMeshPro>();
        icon = modBg.transform.GetChild(1).GetComponent<SpriteRenderer>();
        var nameBg = transform.GetChild(1);
        nameText = nameBg.GetChild(0).GetComponent<TextMeshPro>();
        descRect = desc.gameObject.GetComponent<RectTransform>();

        nameText.font = desc.font = HudManager.Instance.TaskPanel.taskText.font;
        nameText.fontMaterial = desc.fontMaterial = HudManager.Instance.TaskPanel.taskText.fontMaterial;
    }

    // Might not be the best solution, but used to update name/description/icon if updated in the modifier.
    private void FixedUpdate()
    {
        if (Modifier == null || ModifierDisplayComponent.Instance == null || !ModifierDisplayComponent.Instance.IsOpen) return;

        if (Modifier.HideOnUi || Modifier.GetDescription() == string.Empty)
        {
            ModifierDisplayComponent.Instance.DestroyComponent(this);
            return;
        }

        nameText.text = Modifier.ModifierName;
        desc.text = Modifier.GetDescription();
        modBg.gameObject.SetActive(true);

        icon.gameObject.SetActive(Modifier.ModifierIcon != null);
        if (icon.gameObject && icon.gameObject.activeSelf && Modifier.ModifierIcon != null)
        {
            icon.sprite = Modifier.ModifierIcon?.LoadAsset();
            icon.size = new Vector2(0.7f, 0.7f);
            icon.tileMode = SpriteTileMode.Continuous;
            descRect.localPosition = new Vector3(0.38f, -0.1f, -5);
            descRect.sizeDelta = new Vector2(2.38f, 0.65f);
        }
        else
        {
            descRect.localPosition = new Vector3(0, -0.1f, -5);
            descRect.sizeDelta = new Vector2(3.1f, 0.65f);
        }
    }
}
