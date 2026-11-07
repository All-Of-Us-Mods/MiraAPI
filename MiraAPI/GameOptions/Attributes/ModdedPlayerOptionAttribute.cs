using System;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for a <see cref="ModdedPlayerOption"/>.
/// </summary>
/// <param name="title">The title of the option.</param>
/// <param name="allowNone">Whether the option includes a none option. If None is selected, value will return -1.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedPlayerOptionAttribute(string? title, bool allowNone = true)
    : ModdedOptionAttribute(title), ITypedOption<int>, ITypedOption<NetworkedPlayerInfo?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedPlayerOptionAttribute"/> class.
    /// </summary>
    /// <param name="allowNone">Whether the option includes a none option. If None is selected, value will return -1.</param>
    public ModdedPlayerOptionAttribute(bool allowNone = true)
        : this(null, allowNone)
    {
    }

    internal override IModdedOption? CreateOption(object? value, PropertyInfo property, AbstractOptionGroup group)
    {
        var opt = new ModdedPlayerOption(group.GroupPrefix, Title ?? property.Name, allowNone);
        Title = opt.Title;
        return opt;
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        if (HolderOption is not ModdedPlayerOption opt)
            throw new InvalidOperationException($"Holder option for {Title} is not a ModdedPlayerOption.");

        if (ValueType == typeof(int))
            return opt.Value;

        return opt.GetPlayerValue()!;
    }

    /// <inheritdoc />
    public NetworkedPlayerInfo? GetValueUnboxed()
    {
        return HolderOption is ModdedPlayerOption opt
            ? opt.GetPlayerValue()
            : throw new InvalidOperationException($"Holder option for {Title} is not a ModdedPlayerOption.");
    }

    /// <inheritdoc />
    int ITypedOption<int>.GetValueUnboxed()
    {
        return HolderOption is ModdedPlayerOption opt
            ? opt.Value
            : throw new InvalidOperationException($"Holder option for {Title} is not a ModdedPlayerOption.");
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        if (HolderOption is not ModdedPlayerOption opt)
            return;

        var final = value switch
        {
            NetworkedPlayerInfo info => info ? opt.Values.IndexOf(info) : -1,
            _ => (int)value,
        };

        if (final == -1 && !allowNone)
            throw new ArgumentException("Cannot assign -1 or null to a ModdedPlayerOption that does not allow none as an option.");

        opt?.SetValue(final);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(NetworkedPlayerInfo? value)
    {
        if (!value && !allowNone)
            throw new ArgumentException("Cannot assign null to a ModdedPlayerOption that does not allow none as an option.");

        if (HolderOption is ModdedPlayerOption opt)
            opt.SetValue(value ? opt.Values.IndexOf(value!) : -1);
    }

    /// <inheritdoc />
    void ITypedOption<int>.SetValueUnboxed(int value)
    {
        if (value == -1 && !allowNone)
            throw new ArgumentException("Cannot assign -1 to a ModdedPlayerOption that does not allow none as an option.");

        (HolderOption as ModdedPlayerOption)?.SetValue(value);
    }
}
