using System.Reflection;
using MiraAPI.Utilities.Assets;

namespace MiraAPI.Example;

public static class ExampleAssets
{
    private static readonly Assembly _assembly = typeof(MiraAssets).Assembly;

    public static LoadableResourceAsset ExampleButton { get; } = new("MiraAPI.Example.Resources.ExampleButton.png", _assembly);
    public static LoadableResourceAsset CallMeetingButton { get; } = new("MiraAPI.Example.Resources.CallMeeting.png", _assembly);

    // Credit to EpicHorrors for the teleport button asset.
    public static LoadableResourceAsset TeleportButton { get; } = new("MiraAPI.Example.Resources.TeleportButton.png", _assembly);
    public static LoadableResourceAsset SharpshootButton { get; } = new("MiraAPI.Example.Resources.SharpshootButton.png", _assembly);
    public static LoadableResourceAsset Banner { get; } = new("MiraAPI.Example.Resources.FortniteBanner.jpeg", _assembly);
}
