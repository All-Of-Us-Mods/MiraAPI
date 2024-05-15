using BepInEx;
using MiraAPI.Roles;
using Reactor.Localization.Utilities;
using System;

namespace MiraAPI.API.GameOptions;

public abstract class AbstractGameOption
{
    public string Title { get; }
    public StringNames StringName { get; }
    public Type AdvancedRole { get; }
    public bool Save { get; }
    public bool ShowInHideNSeek { get; init; }
    public CustomOptionGroup Group { get; set; }
    public Func<bool> Hidden { get; set; }
    public OptionBehaviour OptionBehaviour { get; protected set; }
    public PluginInfo ParentMod { get; set; }
    public void ValueChanged(OptionBehaviour optionBehaviour)
    {
        OnValueChanged(optionBehaviour);
        CustomOptionsManager.SyncOptions();
    }

    protected abstract void OnValueChanged(OptionBehaviour optionBehaviour);

    protected AbstractGameOption(string title, Type roleType, bool save)
    {
        Title = title;
        StringName = CustomStringName.CreateAndRegister(Title);
        if (roleType is not null && roleType.IsAssignableTo(typeof(ICustomRole)))
        {
            AdvancedRole = roleType;
        }

        Save = save;
        Hidden = () => false;
    }
}