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
public class ModdedPlayerOptionAttribute(
    string title,
    bool allowNone = true)
    : ModdedOptionAttribute(title)
{
    internal override IModdedOption? CreateOption(object? value, PropertyInfo property)
    {
        var option = new ModdedPlayerOption(Title, allowNone);
        return option;
    }

    /// <inheritdoc />
    public override object GetValue()
    {
        if (HolderOption is not ModdedPlayerOption opt)
            throw new InvalidOperationException($"Holder option for {Title} is not a ModdedPlayerOption.");

        if (ValueType == typeof(int))
            return opt.Value;

        return opt.SelectedPlayer!;
    }

    /// <inheritdoc />
    public override void SetValue(object value)
    {
        if (HolderOption is not ModdedPlayerOption opt)
            return;

        var final = value switch
        {
            NetworkedPlayerInfo info => opt.Values.IndexOf(info),
            _ => (int)value,
        };
        opt?.SetValue(final);
    }
}
