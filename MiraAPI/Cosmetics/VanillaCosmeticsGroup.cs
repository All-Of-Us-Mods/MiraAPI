using System.Collections.Generic;

namespace MiraAPI.Cosmetics;
public class VanillaCosmeticsGroup : AbstractCosmeticsGroup
{
    public override string GroupName => "Vanilla";
    internal static HatData[] VanillaHats => DestroyableSingleton<HatManager>.Instance.allHats;
    internal static VisorData[] VanillaVisors => DestroyableSingleton<HatManager>.Instance.allVisors;
    internal static SkinData[] VanillaSkins => DestroyableSingleton<HatManager>.Instance.allSkins;
    internal static NamePlateData[] VanillaNameplates => DestroyableSingleton<HatManager>.Instance.allNamePlates;

    internal List<CosmeticData> Cosmetics { get; } = [];

    internal bool registered { get; set; }
    internal bool runtimeRegister()
    {
        if (registered)
        {
            return true;
        }

        if (!DestroyableSingleton<HatManager>.InstanceExists || VanillaHats.Length == 0
                                                             || VanillaVisors.Length == 0 || VanillaSkins.Length == 0
                                                             || VanillaNameplates.Length == 0)
        {
            return false;
        }

        registered = true;
        Hats.AddRange(VanillaHats);
        Visors.AddRange(VanillaVisors);
        Skins.AddRange(VanillaSkins);
        Nameplates.AddRange(VanillaNameplates);
        Cosmetics.AddRange(Hats);
        Cosmetics.AddRange(Visors);
        Cosmetics.AddRange(Skins);
        Cosmetics.AddRange(Nameplates);
        return true;
    }
}
