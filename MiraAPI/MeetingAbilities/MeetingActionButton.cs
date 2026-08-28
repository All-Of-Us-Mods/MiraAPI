using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.MeetingAbilities;

/// <summary>
/// Class for making custom meeting action buttons.
/// </summary>
public abstract class MeetingActionButton
{
    /// <summary>
    /// Gets the name and text of the button.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the initial cooldown duration in seconds.
    /// </summary>
    public virtual float InitialCooldown => Cooldown;

    /// <summary>
    /// Gets the button's cooldown duration in seconds.
    /// </summary>
    public abstract float Cooldown { get; }

    /// <summary>
    /// Gets the sprite of the button. Use <see cref="LoadableResourceAsset"/> to load a sprite from a resource path. Use <see cref="LoadableBundleAsset{T}"/> to load a sprite from an asset bundle.
    /// </summary>
    public abstract LoadableAsset<Sprite> Sprite { get; }

    /// <summary>
    /// Gets the format string for the cooldown timer.
    /// </summary>
    public virtual string CooldownTimerFormatString => "0";

    /// <summary>
    /// Gets a value indicating whether the button has limited uses.
    /// </summary>
    public bool LimitedUses => ZeroIsInfinite ? MaxUses > 0 : MaxUses >= 0;

    /// <summary>
    /// Gets the maximum number of uses the button has. If the button has infinite uses, set to 0 or -1 based on what value that ZeroIsInfinite is set to.
    /// </summary>
    public virtual int MaxUses => ZeroIsInfinite ? 0 : -1;

    /// <summary>
    /// Gets the value indicating uses mode.
    /// </summary>
    public virtual MeetingButtonUsesMode UsesMode => MeetingButtonUsesMode.PerGame;

    /// <summary>
    /// Gets the button's text outline <see cref="Color"/>.
    /// </summary>
    public virtual Color TextOutlineColor => Color.clear;

    /// <summary>
    /// Gets or sets a value indicating whether limited uses are determined via zero or a negative number of uses.
    /// </summary>
    public virtual bool ZeroIsInfinite { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the timer is currently active.
    /// </summary>
    public bool TimerPaused { get; set; }

    /// <summary>
    /// Gets or sets the amount of uses left.
    /// </summary>
    public int UsesLeft { get; set; }

    /// <summary>
    /// Gets or sets the timer variable to measure cooldowns and effects.
    /// </summary>
    public float Timer { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ActionButton"/> object in game. This is created by Mira API automatically.
    /// </summary>
    public ActionButton? Button { get; set; }

    /// <summary>
    /// The method used to create the button.
    /// </summary>
    /// <param name="parent">The parent of the button.</param>
    public virtual void CreateButton(Transform parent)
    {
        if (Button)
        {
            return;
        }

        Timer = AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay ? 0 : InitialCooldown;
        TimerPaused = false;

        Button = Object.Instantiate(MeetingHud.Instance.MeetingAbilityButton, parent);
        Button.name = Name + "Button";
        Button.OverrideText(Name.ToUpperInvariant());

        Button.graphic.sprite = Sprite.LoadAsset();

        Button.SetUsesRemaining(UsesLeft);
        if (MaxUses <= 0)
        {
            Button.SetInfiniteUses();
        }

        if (TextOutlineColor != Color.clear)
        {
            SetTextOutline(TextOutlineColor);
        }

        var pb = Button.GetComponent<PassiveButton>();
        pb.OnClick = new Button.ButtonClickedEvent();
        pb.OnClick.AddListener((UnityAction)(() =>
        {
            ClickHandler();
        }));
        Button.gameObject.SetActive(Enabled(PlayerControl.LocalPlayer.Data.Role));
    }

    /// <summary>
    /// A utility function to change the outline <see cref="Color"/> of the button's text.
    /// </summary>
    /// <param name="color">The new <see cref="Color"/>.</param>
    public virtual void SetTextOutline(Color color)
    {
        Button?.buttonLabelText.SetOutlineColor(color);
    }

    /// <summary>
    /// A utility function to override the <see cref="UnityEngine.Sprite"/> of the button.
    /// </summary>
    /// <param name="sprite">The new <see cref="UnityEngine.Sprite"/> to override with.</param>
    public virtual void OverrideSprite(Sprite sprite)
    {
        if (Button != null)
        {
            Button.graphic.sprite = sprite;
        }
    }

    /// <summary>
    /// A utility function to override the name of the button.
    /// </summary>
    /// <param name="name">The new name to override with.</param>
    public virtual void OverrideName(string name)
    {
        Button?.OverrideText(name);
    }

    /// <summary>
    /// Set the button's timer.
    /// </summary>
    /// <param name="time">The time you want to set to.</param>
    public virtual void SetTimer(float time)
    {
        Timer = Mathf.Clamp(time, -1, float.MaxValue);
    }

    /// <summary>
    /// Increase the button's timer.
    /// </summary>
    /// <param name="amount">The amount you want to increase by.</param>
    public virtual void IncreaseTimer(float amount)
    {
        SetTimer(Timer + amount);
    }

    /// <summary>
    /// Decrease the button's timer.
    /// </summary>
    /// <param name="amount">The amount you want to decrease by.</param>
    public virtual void DecreaseTimer(float amount)
    {
        SetTimer(Timer - amount);
    }

    /// <summary>
    /// Sets whether the timer is paused or not.
    /// </summary>
    /// <param name="val">Whether you want to pause/resume the timer.</param>
    public virtual void SetTimerPaused(bool val)
    {
        TimerPaused = val;
    }

    /// <summary>
    /// Set the amount of uses this button has left.
    /// </summary>
    /// <param name="amount">The amount you want to set to.</param>
    public virtual void SetUses(int amount)
    {
        UsesLeft = Mathf.Clamp(amount, 0, int.MaxValue);
        Button?.SetUsesRemaining(UsesLeft);

        if (Button != null)
        {
            Button.usesRemainingSprite.color = UsesLeft == 0 ? Color.red : Color.white;
        }
    }

    /// <summary>
    /// Increase the amount of uses this button has left.
    /// </summary>
    /// <param name="amount">The amount you want to increase by. Default: 1.</param>
    public virtual void IncreaseUses(int amount = 1)
    {
        SetUses(UsesLeft + amount);
    }

    /// <summary>
    /// Decrease the amount of uses this button has left.
    /// </summary>
    /// <param name="amount">The amount you want to decrease by. Default: 1.</param>
    public virtual void DecreaseUses(int amount = 1)
    {
        SetUses(UsesLeft - amount);
    }

    /// <summary>
    /// A utility function that runs with the local player's <see cref="MeetingHud.Update"/> if the button is enabled.
    /// </summary>
    /// <param name="hud">the <see cref="MeetingHud"/> instance.</param>
    protected virtual void FixedUpdate(MeetingHud hud)
    {
    }

    /// <summary>
    /// Callback method for the button click event.
    /// </summary>
    protected abstract void OnClick();

    /// <summary>
    /// This method determines if the button should be active or not.
    /// <see langword="true"/> means the button is active, <see langword="false"/> means the button is disabled.
    /// </summary>
    /// <param name="role">The <see cref="RoleBehaviour"/> of the local player.</param>
    /// <returns><see langword="true"/> if the button is enabled, <see langword="false"/> otherwise.</returns>
    public abstract bool Enabled(RoleBehaviour? role);

    /// <summary>
    /// When the button is usable, this method is called to determine if the button can be clicked.
    /// By default, it takes into account the timer, effect, and uses.
    /// You can override it to change the behavior.
    /// </summary>
    /// <returns>A value that represents whether the button can be clicked or not.</returns>
    public virtual bool CanClick()
    {
        return Timer <= 0 && CanUse();
    }

    /// <summary>
    /// Whether the button should light up or not. This is also the base for CanClick.
    /// You can override it to change the behaviour. Do not include timer in here, that is for CanClick.
    /// </summary>
    /// <returns>A value that represents whether the button should light up or not.</returns>
    public virtual bool CanUse()
    {
        return !LimitedUses || UsesLeft > 0;
    }

    /// <summary>
    /// This method handles the button click event. It is a wrapper for the <see cref="OnClick"/> method.
    /// This method takes into account cooldowns, effects, and uses, before calling the <see cref="OnClick"/> method.
    /// It can be overridden for custom behavior.
    /// </summary>
    public virtual void ClickHandler()
    {
        if (!CanClick())
        {
            return;
        }

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
        }

        Timer = Cooldown;
        OnClick();
    }

    /// <summary>
    /// This method is called on the <see cref="MeetingHud.Update"/> method. It is a wrapper for the <see cref="FixedUpdate"/> method.
    /// By default, it handles the cooldown and effect timers, and sets the button to enabled or disabled.
    /// It can be overridden for custom behavior.
    /// </summary>
    /// <param name="hud">The <see cref="MeetingHud"/> instance.</param>
    public virtual void UpdateHandler(MeetingHud hud)
    {
        if (Timer >= 0 && !TimerPaused)
        {
            Timer -= Time.deltaTime;
        }

        if (Button)
        {
            if (CanUse())
            {
                Button!.SetEnabled();
            }
            else
            {
                Button!.SetDisabled();
            }
            Button.SetCooldownFormat(Timer, Cooldown, CooldownTimerFormatString);
            Button.SetCoolDown(Timer, Cooldown);
        }

        FixedUpdate(hud);
    }
}
