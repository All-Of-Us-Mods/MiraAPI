using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MiraAPI.Translation;
using Reactor.Utilities.Attributes;

namespace MiraAPI.GameOptions.Components;

/// <summary>
/// A <see cref="BaseGameSetting"/> implementation that supports multi-select options.
/// </summary>
/// <param name="cppPtr">The C++ pointer to the object.</param>
[RegisterInIl2Cpp]
public class FlagsGameSetting(nint cppPtr) : BaseGameSetting(cppPtr)
{
    /// <summary>
    /// Gets or sets the value currently held by the setting.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Gets or sets the integer value used to denote that all flags have been selected.
    /// </summary>
    public int All { get; set; }

    /// <summary>
    /// Gets or sets every defined flag within the enum.
    /// </summary>
    public Il2CppStructArray<int> Flags { get; set; }

    /// <summary>
    /// Gets or sets the string names associated with every flag.
    /// </summary>
    public Il2CppStructArray<StringNames> Values { get; set; }

    /// <summary>
    /// Gets or sets the string name for when no value is selected.
    /// </summary>
    public StringNames NoneName { get; set; }

    /// <summary>
    /// Gets or sets the string name for when all values are selected.
    /// </summary>
    public StringNames AllName { get; set; }

    /// <inheritdoc/>
    public override string GetValueString(float value)
    {
        return GetValueString((int)value);
    }

    /// <summary>
    /// Gets a value display string.
    /// </summary>
    /// <param name="value">The enum value as an int.</param>
    /// <returns>A value string.</returns>
    public string GetValueString(int value)
    {
        if (value == 0)
        {
            return TranslationController.Instance.GetString(NoneName).Translate();
        }

        if (value == All)
        {
            return TranslationController.Instance.GetString(AllName).Translate();
        }

        var activeCount = 0;
        var firstActiveName = StringNames.None;
        var foundFirst = false;

        for (var i = 0; i < Flags.Length; i++)
        {
            var flag = Flags[i];

            if (flag == 0 || (value & flag) != flag)
                continue;

            if (!foundFirst)
            {
                firstActiveName = Values[i];
                foundFirst = true;
            }

            activeCount++;
        }

        if (activeCount == 0)
        {
            return TranslationController.Instance.GetString(NoneName).Translate();
        }

        var firstNameString = TranslationController.Instance.GetString(firstActiveName).Translate();

        if (activeCount == 1)
        {
            return firstNameString;
        }

        return MiraLocaleManager.GetParsed("MiraApi.Compound", string.Empty, firstNameString, activeCount - 1);
    }
}
