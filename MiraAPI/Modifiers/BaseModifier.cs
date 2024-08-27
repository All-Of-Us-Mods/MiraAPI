namespace MiraAPI.Modifiers;
public abstract class BaseModifier
{
    public abstract string ModifierName { get; }
    public PlayerControl Player { get; internal set; }
    public uint ModifierId { get; internal set; }
    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }
    public virtual void Update() { }
    public virtual void OnDeath() { }
    public virtual bool CanVent() => Player.Data.Role.CanVent;
}