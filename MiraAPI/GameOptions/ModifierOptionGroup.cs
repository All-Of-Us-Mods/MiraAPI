using System.Linq;
using MiraAPI.PluginLoading;

namespace MiraAPI.GameOptions;

[MiraIgnore]
internal sealed class ModifierOptionGroup : AbstractOptionGroup
{
    public override string GroupName { get; }

    public ModifierOptionGroup(string name, IModdedOption[] options, params AbstractOptionGroup[] groups)
    {
        GroupName = name;
        Options.AddRange(options);
        Options.AddRange(groups.SelectMany(x => x.Options));
    }
}
