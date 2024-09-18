using System.Collections.Generic;
using MiraAPI.Cosmetics;

namespace MiraAPI.Example.Cosmetics;
public class ExampleCosmeticGroup : AbstractCosmeticsGroup
{
    public override string GroupName => "Example";

    [RegisterCustomCosmetic()]
    public List<HatData> hats { get; set;  } = new();
    [RegisterCustomCosmetic()]
    public List<NamePlateData> namePlates { get; set;} = new();
    [RegisterCustomCosmetic()]
    public List<SkinData> skins { get; set; } = new();
    [RegisterCustomCosmetic()]
    public List<VisorData> visors { get; set; } = new();
}
