using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.Cosmetics;
using MiraAPI.Example.Cosmetics;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using MiraAPI.Utilities.Assets.Addressable;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace MiraAPI.Example;

[BepInAutoPlugin("mira.example", "MiraExampleMod")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class ExamplePlugin : BasePlugin, IMiraPlugin
{
    public Harmony Harmony { get; } = new(Id);
    public string OptionsTitleText => "Mira API\nExample Mod";
    public ConfigFile GetConfigFile() => Config;
    public override void Load()
    {
        Harmony.PatchAll();
        AddComponent<MiraDebugWindow>();

        new Catalog("Output".ToLocalPath()).LoadCatalog();
        CosmeticGroupSingleton<ExampleCosmeticGroup>.Instance.hats = new LoadableAddressableGroupAsset<HatData>(CatalogTools.GetResourceLocation("hatdata")).LoadAsset();
        CosmeticGroupSingleton<ExampleCosmeticGroup>.Instance.namePlates = new LoadableAddressableGroupAsset<NamePlateData>(CatalogTools.GetResourceLocation("nameplatedata")).LoadAsset();
        CosmeticGroupSingleton<ExampleCosmeticGroup>.Instance.skins = new LoadableAddressableGroupAsset<SkinData>(CatalogTools.GetResourceLocation("skindata")).LoadAsset();
        CosmeticGroupSingleton<ExampleCosmeticGroup>.Instance.visors = new LoadableAddressableGroupAsset<VisorData>(CatalogTools.GetResourceLocation("visordata")).LoadAsset();
    }
}