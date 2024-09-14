using System.Collections.Generic;

namespace MiraAPI.Cosmetics;
public class VanillaCosmeticsGroup : AbstractCosmeticsGroup
{
    public override string GroupName { get; } = "Vanilla";
    internal HatData[] VanillaHats { get { return DestroyableSingleton<HatManager>.Instance.allHats; } }
    internal VisorData[] VanillaVisors { get { return DestroyableSingleton<HatManager>.Instance.allVisors; } }
    internal SkinData[] VanillaSkins { get { return DestroyableSingleton<HatManager>.Instance.allSkins; } }
    internal NamePlateData[] VanillaNameplates { get { return DestroyableSingleton<HatManager>.Instance.allNamePlates; } }

    internal readonly List<CosmeticData> Cosmetics = new();

    internal bool registered { get; set; }
    internal bool runtimeRegister()
    {
        if (registered) return true;
        if (!DestroyableSingleton<HatManager>.InstanceExists || VanillaHats.Length == 0 
            || VanillaVisors.Length == 0 || VanillaSkins.Length == 0 
            || VanillaNameplates.Length == 0) return false;
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