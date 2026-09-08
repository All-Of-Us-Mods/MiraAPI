using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using System;
using System.Collections;
using System.Reflection;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// A number option attribute for the modded options system.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class ModdedNumberOptionListAttribute(
    string title,
    float min,
    float max,
    float increment = 1,
    MiraNumberSuffixes suffixType = MiraNumberSuffixes.None,
    string? formatString = null,
    bool zeroInfinity = false)
    : ModdedOptionListAttribute(title)
{
    internal override IModdedOptionList CreateOptionList(IList value, PropertyInfo property)
    {
        return new ModdedOptionList<ModdedNumberOption>(value.Count, idx =>
        {
            return new(
                GetFormattedTitle(idx),
                (float)(value[idx] ?? min + increment),
                min,
                max,
                increment,
                suffixType,
                formatString,
                zeroInfinity);
        });
    }

    /// <inheritdoc />
    public override void SetValue(int idx, object value)
    {
        var opt = HolderOptionList?[idx] as ModdedNumberOption;
        opt?.SetValue((float)value);
    }

    /// <inheritdoc />
    public override object GetValue(int idx)
    {
        if (HolderOptionList?[idx] is ModdedNumberOption opt)
        {
            return opt.Value;
        }
        throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" is not a ModdedNumberOption");
    }
}
