using MiraAPI.GameOptions.Attributes;

namespace MiraAPI.Example
{
    public class ExOptions
    {
        [RegisterModdedOption("Use Thing")] public bool useThing = false;
    }
}
