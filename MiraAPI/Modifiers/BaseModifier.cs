namespace MiraAPI.Modifiers;
public abstract class BaseModifier
{
    public abstract string ModifierName { get; }
    public uint ModifierId { get; internal set; }
    public PlayerControl Player { get; set; }
    public virtual void OnActivate() { }
    public virtual void OnDeactivate() { }
    public virtual void Update() { }
    public virtual void OnDeath() { }
}