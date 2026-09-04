using System.Globalization;
using Il2CppInterop.Runtime.Attributes;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;

namespace MiraAPI.MeetingAbilities;

/// <summary>
/// A component for handling targeted meeting buttons.
/// </summary>
[RegisterInIl2Cpp]
public class MeetingAbilityBehaviour : MonoBehaviour
{
    /// <summary>
    /// Gets or sets whether the component has been initialized.
    /// </summary>
    private bool _init;

    /// <summary>
    /// The <see cref="TargetedMeetingButton"/> attached to this component.
    /// </summary>
    private TargetedMeetingButton _button = null!;

    /// <summary>
    /// The <see cref="PassiveButton"/> of this component.
    /// </summary>
    public PassiveButton Button;

    /// <summary>
    /// The parent <see cref="PlayerVoteArea"/>.
    /// </summary>
    public PlayerVoteArea VoteArea;

    /// <summary>
    /// The button's <see cref="SpriteRenderer"/> component.
    /// </summary>
    public SpriteRenderer Renderer;

    /// <summary>
    /// The <see cref="TextMeshPro"/> label used for showing the ability's cooldown.
    /// </summary>
    public TextMeshPro CooldownText;

    /// <summary>
    /// The <see cref="TextMeshPro"/> label used for showing the ability's remaining uses.
    /// </summary>
    public TextMeshPro UsesText;

    /// <summary>
    /// Initializes the various properties of this component.
    /// </summary>
    /// <param name="button">The <see cref="TargetedMeetingButton"/> to initialize this component for.</param>
    /// <param name="voteArea">The parent <see cref="PlayerVoteArea"/>.</param>
    [HideFromIl2Cpp]
    public void Initialize(TargetedMeetingButton button, PlayerVoteArea voteArea)
    {
        if (_init) return;
        VoteArea = voteArea;
        _button = button;
        Renderer = GetComponent<SpriteRenderer>();
        SetFillUp(1, 1);
        if (!TryGetComponent(out Button))
        {
            Error($"Could not initialize MeetingButtonBehaviour for {Button.GetType().Name}, Destroying...");
            Destroy(this);
        }
        _init = true;
        CooldownText = Instantiate(HudManager.Instance.KillButton.cooldownTimerText, transform);
        CooldownText.GetComponent<TextTranslatorTMP>().Destroy();
        CooldownText.transform.localPosition = new Vector3(0, 0, -10);
        CooldownText.m_maxFontSize = 3f;
        CooldownText.color = new Color(1, 1, 1, 0.7f);
        UsesText = Instantiate(CooldownText, transform);
        UsesText.transform.localPosition = new Vector3(0.25f, 0.25f, -10);
        UsesText.transform.localScale = Vector3.one * 0.7f;

        CooldownText.gameObject.SetActive(true);
        UsesText.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        if (!_init) return;
        Button.enabled = true;
        Renderer.enabled = true;
        UsesText.enabled = true;
        CooldownText.enabled = true;
        if (_button.Enabled(PlayerControl.LocalPlayer.Data.Role) && _button.IsTargetValid(VoteArea)) return;
        Button.enabled = false;
        Renderer.enabled = false;
        UsesText.enabled = false;
        CooldownText.enabled = false;
    }

    private void Update()
    {
        if (!_init) return;
        if (_button.Timer < 0.01f)
        {
            Renderer.material.SetFloat("_Desat", 0.0f);
            CooldownText.text = string.Empty;
            UsesText.text = _button.LimitedUses ? _button.UsesLeft.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }
        else
        {
            Renderer.material.SetFloat("_Desat", 0.5f);
            CooldownText.text = _button.Timer.ToString(_button.CooldownTimerFormatString, NumberFormatInfo.InvariantInfo);
            UsesText.text = string.Empty;
        }
        SetFillUp(_button.Timer, _button.InitialCooldown);
    }

    public void SetFillUp(float timer, float maxTimer)
    {
        float percentCool = Mathf.Clamp(timer / maxTimer, 0.001f, 1f);
        if (percentCool <= 0) percentCool = 1f;
        Renderer.material.SetFloat("_Percent", percentCool);
    }
}
