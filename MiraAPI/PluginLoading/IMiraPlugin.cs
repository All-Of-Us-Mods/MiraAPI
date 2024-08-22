using BepInEx.Configuration;

namespace MiraAPI.PluginLoading;

public interface IMiraPlugin
{
    public ConfigFile GetConfigFile();
}