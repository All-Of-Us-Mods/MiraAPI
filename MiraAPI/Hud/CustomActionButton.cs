using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Hud;

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
    protected bool EffectActive;

    /// <summary>
    /// A timer variable to measure cooldowns and effects.
    /// </summary>
    protected float Timer;

    /// <summary>
    /// Returns the amount of uses left.
    /// </summary>
    protected int UsesLeft;

    /// <summary>
    /// The button object in game. This is created by Mira API automatically.
    /// </summary>
    protected ActionButton Button { get; private set; }
    
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
        pb.OnClick.RemoveAllListeners();
        pb.OnClick.AddListener((UnityAction)ClickHandler);
    }

    /// <summary>
    /// A utility function to reset the cooldown of the button.
    /// </summary>
    public void ResetCooldown()
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
        Button.graphic.sprite = sprite;
    }

    /// <summary>
    /// A utility function to override the name of the button.
    /// </summary>
    /// <param name="name">The new name to override with</param>
    public void OverrideName(string name)
    {
        Button.OverrideText(name);
    }

    /// <summary>
    /// A utility function that runs with the local PlayerControl's FixedUpdate.
    /// </summary>
    /// <param name="playerControl">the local PlayerControl</param>
    protected virtual void FixedUpdate(PlayerControl playerControl) { }

    /// <summary>
    /// Runs when the button is clicked.
    /// </summary>
    protected abstract void OnClick();

    /// <summary>
    /// This method determines if the button should be active or not.
    /// True means the button is active, false means the button is disabled.
    /// </summary>
    /// <param name="role">The role of the local player.</param>
    public abstract bool Enabled(RoleBehaviour role);

    /// <summary>
    /// Given that there is an effect, this method that runs when the effect ends.
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
        Button.ToggleVisible(visible && Enabled(role));
    }

    /// <summary>
    /// This method handles the button click event. It is a wrapper for the <see cref="OnClick"/> method.
    /// This method takes into account cooldowns, effects, and uses, before calling the <see cref="OnClick"/> method.
    /// It can be overridden for custom behavior.
    /// </summary>
    public virtual void ClickHandler()
    {
        if (!CanUse())
        {
            return;
        }

        if (LimitedUses)
        {
            UsesLeft--;
            Button.SetUsesRemaining(UsesLeft);
        }

        OnClick();
        Button.SetDisabled();
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
            Button.SetEnabled();
        }
        else
        {
            Button.SetDisabled();
        }
        
        Button.SetCoolDown(Timer, EffectActive ? EffectDuration : Cooldown);

        FixedUpdate(playerControl);
    }
}