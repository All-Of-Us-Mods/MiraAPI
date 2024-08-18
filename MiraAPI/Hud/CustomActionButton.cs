using MiraAPI.Utilities.Assets;
using UnityEngine;
using UnityEngine.Events;

namespace MiraAPI.Hud;

public abstract class CustomActionButton
{
    public abstract string Name { get; }

    public abstract float Cooldown { get; }

    public abstract float EffectDuration { get; }

    public abstract int MaxUses { get; }

    public abstract LoadableAsset<Sprite> Sprite { get; }

    public bool HasEffect => EffectDuration > 0;

    public bool LimitedUses => MaxUses > 0;

    protected bool EffectActive;

    protected float Timer;

    protected int UsesLeft;

    protected ActionButton Button;

    public void CreateButton(Transform parent)
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

    public void ResetCooldown()
    {
        Timer = Cooldown;
        if (EffectActive)
        {
            OnEffectEnd();
        }
        
        EffectActive = false;
    }
    
    public void OverrideSprite(Sprite sprite)
    {
        Button.graphic.sprite = sprite;
    }

    public void OverrideName(string name)
    {
        Button.OverrideText(name);
    }

    protected virtual void FixedUpdate(PlayerControl playerControl) { }

    protected abstract void OnClick();

    public abstract bool Enabled(RoleBehaviour role);

    public virtual void OnEffectEnd() { }

    public virtual bool CanUse()
    {
        return Timer <= 0 && !EffectActive && (!LimitedUses || UsesLeft > 0);
    }

    public virtual void SetActive(bool visible, RoleBehaviour role)
    {
        Button.ToggleVisible(visible && Enabled(role));
    }

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

    public void FixedUpdateHandler(PlayerControl playerControl)
    {
        if (Timer >= 0)
        {
            Timer -= Time.deltaTime;
        }
        else
        {
            if (HasEffect && EffectActive)
            {
                EffectEndHandler();
            }
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
    
    private void EffectEndHandler()
    {
        EffectActive = false;
        Timer = Cooldown;
        OnEffectEnd();
    }
}