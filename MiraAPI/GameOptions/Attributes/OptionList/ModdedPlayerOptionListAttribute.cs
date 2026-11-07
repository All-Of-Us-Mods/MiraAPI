using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of player options.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="allowNone">Whether the option includes a none option. If None is selected, value will return -1.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedPlayerOptionListAttribute(string? title, bool allowNone = true)
    : ModdedOptionListAttribute(title), ITypedOptionList<int>, ITypedOptionList<NetworkedPlayerInfo?>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedPlayerOptionListAttribute"/> class.
    /// </summary>
    /// <param name="allowNone">Whether the option includes a none option. If None is selected, value will return -1.</param>
    public ModdedPlayerOptionListAttribute(bool allowNone = true)
        : this(null, allowNone)
    {
    }

    internal override IModdedOptionList CreateOptionList(IList list, PropertyInfo property, AbstractOptionGroup group)
    {
        return new ModdedOptionList<ModdedPlayerOption>(list.Count, idx =>
        {
            return new(group.GroupPrefix, GetFormattedTitle(idx, property), allowNone);
        });
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        if (HolderOptionList?[idx] is not ModdedPlayerOption opt)
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
    public void SetValueUnboxed(int idx, NetworkedPlayerInfo? value)
    {
        if (!value && !allowNone)
            throw new ArgumentException("Cannot assign null to a ModdedPlayerOption that does not allow none as an option.");

        if (HolderOptionList?[idx] is ModdedPlayerOption opt)
            opt.SetValue(value ? opt.Values.IndexOf(value!) : -1);
    }

    /// <inheritdoc />
    public void SetValueUnboxed(int idx, int value)
    {
        if (value == -1 && !allowNone)
            throw new ArgumentException("Cannot assign -1 to a ModdedPlayerOption that does not allow none as an option.");

        (HolderOptionList?[idx] as ModdedPlayerOption)?.SetValue(value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        if (HolderOptionList?[idx] is not ModdedPlayerOption opt)
            throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedPlayerOption");

        if (ValueType == typeof(int))
            return opt.Value;

        return opt.GetPlayerValue()!;
    }

    /// <inheritdoc />
    public NetworkedPlayerInfo? GetValueUnboxed(int idx)
    {
        return HolderOptionList?[idx] is ModdedPlayerOption opt
            ? opt.GetPlayerValue()
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedPlayerOption");
    }

    /// <inheritdoc />
    int ITypedOptionList<int>.GetValueUnboxed(int idx)
    {
        return HolderOptionList?[idx] is ModdedPlayerOption opt
            ? opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{Titles[idx]}\" is not a ModdedPlayerOption");
    }
}
