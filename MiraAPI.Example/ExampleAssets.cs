using MiraAPI.Utilities.Assets;

namespace MiraAPI.Example;

public static class ExampleAssets
{
    public static LoadableResourceAsset ExampleButton { get; } = new("MiraAPI.Example.Resources.ExampleButton.png");
    public static LoadableResourceAsset CallMeetingButton { get; } = new("MiraAPI.Example.Resources.CallMeeting.png");

    // Credit to EpicHorrors for the teleport button asset.
    public static LoadableResourceAsset TeleportButton { get; } = new("MiraAPI.Example.Resources.TeleportButton.png");
    public static LoadableResourceAsset SharpshootButton { get; } = new("MiraAPI.Example.Resources.SharpshootButton.png");
    public static LoadableResourceAsset Banner { get; } = new("MiraAPI.Example.Resources.FortniteBanner.jpeg");
}
