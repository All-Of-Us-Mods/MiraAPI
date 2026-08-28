using MiraAPI.PluginLoading;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.GameModes;

/// <summary>
/// The classic game mode.
/// </summary>
[MiraIgnore]
public class ClassicMode : AbstractGameMode
{
    /// <inheritdoc/>
    public override string Name => "MiraApi.Gamemode.Classic";

    /// <inheritdoc/>
    public override string Description => "MiraApi.Gamemode.Classic.Description";

    public override LoadableAsset<Sprite>? Icon => MiraAssets.ClassicGamemodeIcon;
}
