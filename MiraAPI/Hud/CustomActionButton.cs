using System;
using System.Globalization;
using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Keybinds;
using MiraAPI.Patches;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MiraAPI.Hud;

/// <summary>
/// Class for making custom action buttons. More customizable than the default Action/Ability buttons in the base game.
/// </summary>
public abstract class CustomActionButton
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
    /// Gets a value indicating whether the timer should stop decreasing when the player is venting.
    /// Note that both cooldown decrease and effect timer decrease is disabled.
    /// </summary>
    public virtual bool PauseTimerInVent => false;

    /// <summary>
    /// Gets the sprite of the button. Use <see cref="LoadableResourceAsset"/> to load a sprite from a resource path. Use <see cref="LoadableBundleAsset{T}"/> to load a sprite from an asset bundle.
    /// </summary>
    public abstract LoadableAsset<Sprite> Sprite { get; }

    /// <summary>
    /// Gets the format string for the cooldown timer.
    /// </summary>
    public virtual string CooldownTimerFormatString => "0";

    /// <summary>
    /// Gets a value indicating whether the button has an effect ability. By default true if <see cref="EffectDuration"/> is greater than 0.
    /// </summary>
    public virtual bool HasEffect => EffectDuration > 0;

    /// <summary>
    /// Gets the button's effect duration in seconds. If 0, the effect needs to be ended manually.
    /// </summary>
    public virtual float EffectDuration => 0;

    /// <summary>
    /// Gets the maximum amount of uses the button has. If the button has infinite uses, set to 0.
    /// </summary>
    public virtual int MaxUses => 0;

    /// <summary>
    /// Gets the value indicating uses mode.
    /// </summary>
    public virtual ButtonUsesMode UsesMode => ButtonUsesMode.PerGame;

    /// <summary>
    /// Gets the keybind for this button. If null, no keybind will be added.
    /// </summary>
    public virtual MiraKeybind? Keybind => null;

    /// <summary>
    /// Gets the button's text outline color.
    /// </summary>
    public virtual Color TextOutlineColor => Color.clear;

    /// <summary>
    /// Gets or sets the location of the button on the screen.
    /// </summary>
    public virtual ButtonLocation Location { get; set; } = ButtonLocation.BottomLeft;

    /// <summary>
    /// Gets a value indicating whether the button has limited uses.
    /// </summary>
    public bool LimitedUses => MaxUses > 0;

    /// <summary>
    /// Gets or sets a value indicating whether the effect is currently active, if there is one.
    /// </summary>
    public bool EffectActive { get; set; }

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
    /// Gets or sets the button object in game. This is created by Mira API automatically.
    /// </summary>
    public ActionButton? Button { get; set; }

    /// <summary>
    /// Gets the gameObject used for the keybind icon.
    /// </summary>
    public GameObject? KeybindIcon { get; private set; }

    /// <summary>
    /// Gets or sets the keybind icon text.
    /// </summary>
    private TextMeshPro? KeybindText { get; set; }

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

        UsesLeft = MaxUses;
        Timer = AmongUsClient.Instance?.NetworkMode == NetworkModes.FreePlay ? 0 : InitialCooldown;
        EffectActive = false;
        TimerPaused = false;

        Button = Object.Instantiate(HudManager.Instance.AbilityButton, parent);
        Button.name = Name + "Button";
        Button.OverrideText(Name.ToUpperInvariant());

        Button.graphic.sprite = Sprite.LoadAsset();

        Button.SetUsesRemaining(MaxUses);
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
            // Invoke the generic button click event.
            var genericEvent = new MiraButtonClickEvent(this);
            MiraEventManager.InvokeEvent(genericEvent);
            if (genericEvent.IsCancelled)
            {
                MiraEventManager.InvokeEvent(new MiraButtonCancelledEvent(this));
            }

            // Invoke the button click event for specific button.
            var eventType = CustomButtonManager.ButtonEventTypes[GetType()];
            var @event = (MiraCancelableEvent)Activator.CreateInstance(eventType, this, genericEvent)!;
            var specificInvoked = MiraEventManager.InvokeEvent(@event, eventType);
            if (@event.IsCancelled)
            {
                var cancelEventType = CustomButtonManager.ButtonCancelledEventTypes[GetType()];
                var cancelEvent = (MiraEvent)Activator.CreateInstance(cancelEventType, this)!;
                MiraEventManager.InvokeEvent(cancelEvent, cancelEventType);
            }

            if (specificInvoked)
            {
                if (!@event.IsCancelled)
                {
                    ClickHandler();
                }
            }
            else
            {
                if (!genericEvent.IsCancelled)
                {
                    ClickHandler();
                }
            }
        }));

        if (Keybind != null)
        {
            Keybind.OnActivate(() =>
            {
                if (Enabled(PlayerControl.LocalPlayer.Data.Role))
                {
                    ClickHandler();
                }
            });

            KeybindIcon =
                Helpers.CreateKeybindIcon(
                    Button.gameObject,
                    Keybind.CurrentKey,
                    new Vector3(MaxUses <= 0 ? -0.4f : 0.4f, 0.45f, -9f)
                );
            KeybindText = KeybindIcon.transform.GetChild(0).GetComponent<TextMeshPro>();
            Button.usesRemainingSprite.transform.localPosition = new(-0.341f, 0.45f, -0.1f);
        }
    }

    /// <summary>
    /// Allows you to change the button's location.
    /// </summary>
    /// <param name="location">The new location.</param>
    /// <param name="moveButton">Whether the button's position should change in-game.</param>
    public virtual void SetButtonLocation(ButtonLocation location, bool moveButton = true)
    {
        if (!HudManager.InstanceExists || Button == null)
        {
            return;
        }

        this.Location = location;

        if (!moveButton)
        {
            return;
        }

        if (HudManagerPatches.BottomLeft == null || HudManagerPatches.BottomRight == null)
        {
            return;
        }

        switch (location)
        {
            case ButtonLocation.BottomLeft:
                var gridArrange = HudManagerPatches.BottomLeft.GetComponent<GridArrange>();
                var aspectPosition = HudManagerPatches.BottomLeft.GetComponent<AspectPosition>();

                Button.transform.SetParent(HudManagerPatches.BottomLeft.transform);

                gridArrange.Start();
                gridArrange.ArrangeChilds();
                aspectPosition.AdjustPosition();
                break;
            case ButtonLocation.BottomRight:
                Button.transform.SetParent(HudManagerPatches.BottomRight);
                break;
        }
    }

    /// <summary>
    /// A utility function to reset the cooldown and/or effect of the button.
    /// </summary>
    public virtual void ResetCooldownAndOrEffect()
    {
        Timer = Cooldown;
        if (EffectActive)
        {
            OnEffectEnd();
        }

        EffectActive = false;
    }

    /// <summary>
    /// A utility function to change the outline color of the button's text.
    /// </summary>
    /// <param name="color">The new color.</param>
    public virtual void SetTextOutline(Color color)
    {
        Button?.buttonLabelText.SetOutlineColor(color);
    }

    /// <summary>
    /// A utility function to override the sprite of the button.
    /// </summary>
    /// <param name="sprite">The new sprite to override with.</param>
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
    /// A utility function that runs with the local PlayerControl's FixedUpdate if the button is enabled.
    /// </summary>
    /// <param name="playerControl">the local PlayerControl.</param>
    protected virtual void FixedUpdate(PlayerControl playerControl)
    {
    }

    /// <summary>
    /// Callback method for the button click event.
    /// </summary>
    protected abstract void OnClick();

    /// <summary>
    /// This method determines if the button should be active or not.
    /// True means the button is active, false means the button is disabled.
    /// </summary>
    /// <param name="role">The role of the local player.</param>
    /// <returns>True if the button is enabled, false otherwise.</returns>
    public abstract bool Enabled(RoleBehaviour? role);

    /// <summary>
    /// Given that there is an effect, this method runs when the effect ends.
    /// <br /> <br /> THIS IS A CALLBACK METHOD! Use <see cref="ResetCooldownAndOrEffect" /> if you want to end the effect.
    /// </summary>
    public virtual void OnEffectEnd()
    {
    }

    /// <summary>
    /// Returns a value indicating whether the effect can be ended early by the player.
    /// Always false by default.
    /// </summary>
    /// <returns>Can the effect be canceled.</returns>
    public virtual bool IsEffectCancellable() => false;

    /// <summary>
    /// When the button is usable, this method is called to determine if the button can be clicked.
    /// By default, it takes into account the timer, effect, and uses.
    /// You can override it to change the behavior.
    /// </summary>
    /// <returns>A value that represents whether the button can be clicked or not.</returns>
    public virtual bool CanClick()
    {
        return (EffectActive ? IsEffectCancellable() : Timer <= 0) && CanUse();
    }

    /// <summary>
    /// Whether the button should light up or not. This is also the base for CanClick.
    /// You can override it to change the behaviour. Do not include timer in here, that is for CanClick.
    /// </summary>
    /// <returns>A value that represents whether the button should light up or not.</returns>
    public virtual bool CanUse()
    {
        return PlayerControl.LocalPlayer.moveable &&
               ((EffectActive && IsEffectCancellable()) || (!EffectActive && (!LimitedUses || UsesLeft > 0)));
    }

    /// <summary>
    /// This method is called on the HudManager.SetHudActive method. It determines whether the button should be visible or not.
    /// The default behavior is to show the button if the <paramref name="visible"/> parameter and the <see cref="Enabled"/> method return true.
    /// It can be overridden for custom behavior.
    /// </summary>
    /// <param name="visible">Passed in from HudManager.SetHudActive, should hud be active.</param>
    /// <param name="role">Passed in from HudManager.SetHudActive, the current role of the player.</param>
    public virtual void SetActive(bool visible, RoleBehaviour role)
    {
        Button?.ToggleVisible(visible && Enabled(role));
    }

    /// <summary>
    /// This method handles the button click event. It is a wrapper for the <see cref="OnClick"/> method.
    /// This method takes into account cooldowns, effects, and uses, before calling the <see cref="OnClick"/> method.
    /// It can be overridden for custom behavior.
    /// </summary>
    public virtual void ClickHandler()
    {
        if (EffectActive && IsEffectCancellable())
        {
            ResetCooldownAndOrEffect();
            return;
        }

        if (!CanClick())
        {
            return;
        }

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
        }

        OnClick();

        if (HasEffect)
        {
            EffectActive = true;
            Timer = EffectDuration;
        }
        else
        {
            Timer = Cooldown;
        }
    }

    /// <summary>
    /// This method is called on the PlayerControl.FixedUpdate method. It is a wrapper for the <see cref="FixedUpdate"/> method.
    /// By default, it handles the cooldown and effect timers, and sets the button to enabled or disabled.
    /// It can be overridden for custom behavior.
    /// </summary>
    /// <param name="playerControl">The local PlayerControl.</param>
    public virtual void FixedUpdateHandler(PlayerControl playerControl)
    {
        if (Keybind != null && KeybindText != null)
        {
            KeybindText.text = Keybind.CurrentKey.ToString();
            KeybindIcon?.SetActive(ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard &&
                                   PluginSingleton<MiraApiPlugin>.Instance.MiraConfig!.ShowKeybinds.Value);
        }

        if (Timer >= 0)
        {
            if (!TimerPaused && (!PauseTimerInVent || !playerControl.inVent))
            {
                Timer -= Time.deltaTime;
            }
        }
        else if (HasEffect && EffectActive && EffectDuration > 0)
        {
            EffectActive = false;
            Timer = Cooldown;
            OnEffectEnd();
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

            if (EffectActive && EffectDuration > 0)
            {
                Button.SetFillUp(Timer, EffectDuration);

                Button.cooldownTimerText.text = Timer.ToString(CooldownTimerFormatString, NumberFormatInfo.InvariantInfo);
                Button.cooldownTimerText.gameObject.SetActive(true);
            }
            else
            {
                Button.SetCooldownFormat(Timer, Cooldown, CooldownTimerFormatString);
            }
        }

        FixedUpdate(playerControl);
    }
}

/// <summary>
/// Custom action button that has a target object.
/// </summary>
/// <typeparam name="T">The type of the target object.</typeparam>
public abstract class CustomActionButton<T> : CustomActionButton where T : MonoBehaviour
{
    /// <summary>
    /// Gets or sets the target object of the button.
    /// </summary>
    public T? Target { get; set; }

    /// <summary>
    /// Gets the distance the player must be from the target object to use the button.
    /// </summary>
    public virtual float Distance => PlayerControl.LocalPlayer.Data.Role.GetAbilityDistance();

    /// <summary>
    /// Determines if the target object is valid.
    /// </summary>
    /// <param name="target">The target object being checked.</param>
    /// <returns>True if the target object is valid, false otherwise.</returns>
    public virtual bool IsTargetValid(T? target)
    {
        return target != null;
    }

    /// <summary>
    /// The method used to get the target object.
    /// </summary>
    /// <returns>The target object or null if it isn't found.</returns>
    public abstract T? GetTarget();

    /// <summary>
    /// Sets the outline of the target object.
    /// </summary>
    /// <param name="active">Should the outline be active.</param>
    public abstract void SetOutline(bool active);

    /// <inheritdoc />
    public override bool CanUse()
    {
        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            SetOutline(false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        return base.CanUse() && Target != null;
    }

    /// <inheritdoc />
    public override bool CanClick()
    {
        return base.CanClick() && Target != null;
    }

    /// <summary>
    /// Use this to reset the button's target after used.
    /// </summary>
    public virtual void ResetTarget()
    {
        Target = null;
        SetOutline(false);
    }
}
