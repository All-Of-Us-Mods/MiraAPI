using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A static class that contains various assets used in the Mira API.
/// </summary>
public static class MiraAssets
{
    static MiraAssets()
    {
        var boxTex = SpriteTools.LoadTextureFromResourcePath(
            "MiraAPI.Resources.RoundedBox.png",
            System.Reflection.Assembly.GetExecutingAssembly());
        var boxSprite = Sprite.Create(
            boxTex,
            new Rect(0, 0, boxTex.width, boxTex.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0U,
            SpriteMeshType.Tight,
            new Vector4(20, 20, 20, 20));

        RoundedBox = new LoadableAssetWrapper<Sprite>(boxSprite);
    }

    /// <summary>
    /// Gets the color used for teal highlighting in UI.
    /// </summary>
    public static Color32 AcceptedTeal { get; } = new(43, 233, 198, 255);

    /// <summary>
    /// Gets the Mira API asset bundle.
    /// </summary>
    public static AssetBundle MiraAssetBundle { get; } = AssetBundleManager.Load("mirabundle");

    /// <summary>
    /// Gets the ModifierDisplay prefab.
    /// </summary>
    public static LoadableAsset<GameObject> ModifierDisplay { get; } = new LoadableBundleAsset<GameObject>("Modifiers", MiraAssetBundle);

    /// <summary>
    /// Gets the Popup prefab for saving presets.
    /// </summary>
    public static LoadableAsset<GameObject> PresetSavePopup { get; } = new LoadableBundleAsset<GameObject>("PresetSavePopup", MiraAssetBundle);

    /// <summary>
    /// Gets the Refresh Icon sprite.
    /// </summary>
    public static LoadableAsset<Sprite> RefreshIcon { get; } = new LoadableBundleAsset<Sprite>("refresh", MiraAssetBundle);

    /// <summary>
    /// Gets the Folder Icon sprite.
    /// </summary>
    public static LoadableAsset<Sprite> FolderIcon { get; } = new LoadableBundleAsset<Sprite>("freePlay_folderTaskRoom", MiraAssetBundle);

    /// <summary>
    /// Gets the empty sprite asset.
    /// </summary>
    public static LoadableResourceAsset Empty { get; } = new("MiraAPI.Resources.Empty.png");

    /// <summary>
    /// Gets the RoundedBox sprite, which is a rounded rectangle used for UI elements.
    /// </summary>
    public static LoadableAsset<Sprite> RoundedBox { get; }

    /// <summary>
    /// Gets the Next Button sprite.
    /// </summary>
    public static LoadableResourceAsset NextButton { get; } = new("MiraAPI.Resources.NextButton.png");

    /// <summary>
    /// Gets the highlighted Next Button sprite.
    /// </summary>
    public static LoadableResourceAsset NextButtonActive { get; } = new("MiraAPI.Resources.NextButtonActive.png");

    /// <summary>
    /// Gets the Cog icon used in Role Settings Menu.
    /// </summary>
    public static LoadableResourceAsset Cog { get; } = new("MiraAPI.Resources.Cog.png");

    /// <summary>
    /// Gets the Checkmark Box used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset CheckmarkBox { get; } = new("MiraAPI.Resources.CheckMarkBox.png");

    /// <summary>
    /// Gets the Checkmark used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset Checkmark { get; } = new("MiraAPI.Resources.Checkmark.png");

    /// <summary>
    /// Gets the white CategoryHeader used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset CategoryHeader { get; } = new("MiraAPI.Resources.CategoryHeader.png");

    /// <summary>
    /// Gets the sprite used for the Keybind icons.
    /// </summary>
    public static LoadableResourceAsset KeybindButton { get; } = new("MiraAPI.Resources.KeybindButton.png");
}
