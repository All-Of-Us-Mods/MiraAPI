using BepInEx.Configuration;

namespace MiraAPI.PluginLoading;

public interface IMiraPlugin
{
    string OptionsTitleText { get; }
    public ConfigFile GetConfigFile();
}