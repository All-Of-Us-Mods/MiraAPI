namespace MiraAPI.Utilities.Assets
{
    public static class MiraAssets
    {
        public static readonly LoadableResourceAsset Empty = new("", "");

        public static readonly LoadableResourceAsset NextButton = new("NextButton.png", "MiraAPI.Resources.");
        public static readonly LoadableResourceAsset NextButtonActive = new("NextButtonActive.png", "MiraAPI.Resources.");
    }
}
