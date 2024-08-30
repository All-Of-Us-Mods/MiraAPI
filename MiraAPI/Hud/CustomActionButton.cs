using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MiraAPI.Hud;

/// <summary>
/// Class for making custom action buttons. More customizable than the default Action/Ability buttons in the base game
/// </summary>
public abstract class CustomActionButton
{
    /// <summary>
    /// The name and text of the button.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// How long the button takes to cooldown.
    /// </summary>
    public abstract float Cooldown { get; }

    /// <summary>
    /// If the button has an effect ability, how long the effect should last. To disable effects, set to 0.
    /// </summary>
    public abstract float EffectDuration { get; }

    /// <summary>
    /// If the button has limited uses, how many uses it has. To make the button infinite, set to 0.
    /// </summary>
    public abstract int MaxUses { get; }

    /// <summary>
    /// The sprite of the button. Use <see cref="LoadableResourceAsset"/> to load a sprite from a resource path. Use <see cref="LoadableBundleAsset{T}"/> to load a sprite from an asset bundle.
    /// </summary>
    public abstract LoadableAsset<Sprite> Sprite { get; }

    /// <summary>
    /// The location of the button on the screen
    /// </summary>
    public virtual ButtonLocation Location => ButtonLocation.BottomLeft;

    /// <summary>
    /// Returns true if the button has an effect ability.
    /// </summary>
    public bool HasEffect => EffectDuration > 0;

    /// <summary>
    /// Returns true if the button has limited uses.
    /// </summary>
    public bool LimitedUses => MaxUses > 0;

    /// <summary>
    /// Returns true if the effect is currently active.
    /// </summary>
    public bool EffectActive { get; protected set; }

    /// <summary>
    /// Returns the amount of uses left.
    /// </summary>
    public int UsesLeft { get; protected set; }

    /// <summary>
    /// A timer variable to measure cooldowns and effects.
    /// </summary>
    public float Timer { get; protected set; }

    /// <summary>
    /// The button object in game. This is created by Mira API automatically.
    /// </summary>
    protected ActionButton? Button { get; private set; }

    internal void CreateButton(Transform parent)
    {
        if (Button)
        {
            return;
        }

        UsesLeft = MaxUses;
        Timer = Cooldown;
        EffectActive = false;

        Button = Object.Instantiate(HudManager.Instance.AbilityButton, parent);
        Button.name = Name + "Button";
        Button.OverrideText(Name.ToUpper());

        Button.graphic.sprite = Sprite.LoadAsset();

        Button.SetUsesRemaining(MaxUses);
        if (MaxUses <= 0)
        {
            Button.SetInfiniteUses();
        }

        var pb = Button.GetComponent<PassiveButton>();
        pb.OnClick = new Button.ButtonClickedEvent();
        pb.OnClick.AddListener((UnityAction)ClickHandler);
    }

    /// <summary>
    /// A utility function to reset the cooldown and/or effect of the button.
    /// </summary>
    public void ResetCooldownAndOrEffect()
    {
        Timer = Cooldown;
        if (EffectActive)
        {
            OnEffectEnd();
        }

        EffectActive = false;
    }

    /// <summary>
    /// A utility function to override the sprite of the button.
    /// </summary>
    /// <param name="sprite">The new sprite to override with</param>
    public void OverrideSprite(Sprite sprite)
    {
        if (Button != null)
        {
            Button.graphic.sprite = sprite;
        }
    }

    /// <summary>
    /// A utility function to override the name of the button.
    /// </summary>
    /// <param name="name">The new name to override with</param>
    public void OverrideName(string name)
    {
        Button?.OverrideText(name);
    }

    /// <summary>
    /// Set the button's timer.
    /// </summary>
    /// <param name="amount">The amount you want to set to.</param>
    public void SetTimer(float time)
    {
        Timer = Mathf.Clamp(time, -1, float.MaxValue);
    }

    /// <summary>
    /// Increase the button's timer.
    /// </summary>
    /// <param name="amount">The amount you want to increase by.</param>
    public void IncreaseTimer(float amount)
    {
        SetTimer(Timer + amount);
    }

    /// <summary>
    /// Decrease the button's timer.
    /// </summary>
    /// <param name="amount">The amount you want to decrease by.</param>
    public void DecreaseTimer(float amount)
    {
        SetTimer(Timer - amount);
    }


    /// <summary>
    /// Set the amount of uses this button has left.
    /// </summary>
    /// <param name="amount">The amount you want to set to.</param>
    public void SetUses(int amount)
    {
        UsesLeft = Mathf.Clamp(amount, 0, int.MaxValue);
    }

    /// <summary>
    /// Increase the amount of uses this button has left.
    /// </summary>
    /// <param name="amount">The amount you want to increase by. Default: 1</param>
    public void IncreaseUses(int amount = 1)
    {
        SetUses(UsesLeft + amount);
    }

    /// <summary>
    /// Decrease the amount of uses this button has left.
    /// </summary>
    /// <param name="amount">The amount you want to decrease by. Default: 1</param>
    public void DecreaseUses(int amount = 1)
    {
        SetUses(UsesLeft - amount);
    }

    /// <summary>
    /// A utility function that runs with the local PlayerControl's FixedUpdate if the button is enabled.
    /// </summary>
    /// <param name="playerControl">the local PlayerControl</param>
    protected virtual void FixedUpdate(PlayerControl playerControl) { }

    /// <summary>
    /// Callback method for the button click event.
    /// </summary>
    protected abstract void OnClick();

    /// <summary>
    /// This method determines if the button should be active or not.
    /// True means the button is active, false means the button is disabled.
    /// </summary>
    /// <param name="role">The role of the local player.</param>
    public abstract bool Enabled(RoleBehaviour? role);

    /// <summary>
    /// Given that there is an effect, this method runs when the effect ends.
    /// <br /> <br /> THIS IS A CALLBACK METHOD! Use <see cref="ResetCooldownAndOrEffect" /> if you want to end the effect.
    /// </summary>
    public virtual void OnEffectEnd() { }

    /// <summary>
    /// When the button is enabled, this method is called to determine if the button can be used.
    /// By default, it takes into account the timer, effect, and uses.
    /// You can override it to change the behavior.
    /// </summary>
    /// <returns>A value that represents whether the button can be used or not.</returns>
    public virtual bool CanUse()
    {
        return Timer <= 0 && !EffectActive && (!LimitedUses || UsesLeft > 0);
    }

    /// <summary>
    /// This method is called on the HudManager.SetHudActive method. It determines whether the button should be visible or not.
    /// The default behavior is to show the button if the <paramref name="visible"/> parameter and the <see cref="Enabled"/> method return true.
    /// It can be overridden for custom behavior.
    /// </summary>
    /// <param name="visible">Passed in from HudManager.SetHudActive, should hud be active?</param>
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
    protected virtual void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        if (LimitedUses)
        {
            UsesLeft--;
            Button?.SetUsesRemaining(UsesLeft);
        }

        OnClick();
        Button?.SetDisabled();
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
    /// <param name="playerControl">The local PlayerControl</param>
    public virtual void FixedUpdateHandler(PlayerControl playerControl)
    {
        if (Timer >= 0)
        {
            Timer -= Time.deltaTime;
        }
        else if (HasEffect && EffectActive)
        {
            EffectActive = false;
            Timer = Cooldown;
            OnEffectEnd();
        }

        if (CanUse())
        {
            Button?.SetEnabled();
        }
        else
        {
            Button?.SetDisabled();
        }

        Button?.SetCoolDown(Timer, EffectActive ? EffectDuration : Cooldown);

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
    /// The target object of the button.
    /// </summary>
    public T? Target { get; private set; }

    /// <summary>
    /// The distance the player must be from the target object to use the button.
    /// </summary>
    public virtual float Distance => PlayerControl.LocalPlayer.Data.Role.GetAbilityDistance();

    /// <summary>
    /// An optional collider tag to filter the target object by.
    /// </summary>
    public virtual string? ColliderTag => null;

    /// <summary>
    /// Determines if the target object is valid.
    /// </summary>
    public virtual bool IsTargetValid(T? target)
    {
        return target;
    }

    /// <summary>
    /// The method used to get the target object.
    /// </summary>
    public abstract T? GetTarget();

    /// <summary>
    /// Sets the outline of the target object.
    /// </summary>
    /// <param name="active">Should the outline be active</param>
    public abstract void SetOutline(bool active);


    public override bool CanUse()
    {
        var newTarget = GetTarget();
        if (newTarget != Target)
        {
            SetOutline(false);
        }

        Target = IsTargetValid(newTarget) ? newTarget : null;
        SetOutline(true);

        return base.CanUse() && Target;
    }
}