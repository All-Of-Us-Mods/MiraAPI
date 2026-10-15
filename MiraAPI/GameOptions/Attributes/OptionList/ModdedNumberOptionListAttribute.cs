using System;
using System.Collections;
using System.Reflection;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;

namespace MiraAPI.GameOptions.Attributes;

/// <summary>
/// Attribute for creating a list of number options.
/// </summary>
/// <param name="title">The option title.</param>
/// <param name="min">The minimum value.</param>
/// <param name="max">The maximum value.</param>
/// <param name="increment">The increment.</param>
/// <param name="suffixType">The suffix type.</param>
/// <param name="formatString">Optional format string for the option screen.</param>
/// <param name="zeroInfinity">Flag indicating whether 0 means infinity.</param>
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
        return HolderOptionList?[idx] is ModdedNumberOption opt
            ? (object)opt.Value
            : throw new InvalidOperationException($"HolderOption for option \"{GetFormattedTitle(idx)}\" is not a ModdedNumberOption");
    }
}
