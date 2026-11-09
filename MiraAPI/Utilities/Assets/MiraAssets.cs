using System.Reflection;
using Reactor.Utilities;
using UnityEngine;

namespace MiraAPI.Utilities.Assets;

/// <summary>
/// A static class that contains various assets used in the Mira API.
/// </summary>
public static class MiraAssets
{
    private static readonly Assembly Assembly = typeof(MiraAssets).Assembly;

    static MiraAssets()
    {
        var boxTex = SpriteTools.LoadTextureFromResourcePath(
            "MiraAPI.Resources.RoundedBox.png",
            Assembly);
        var boxSprite = Sprite.Create(
            boxTex,
            new Rect(0, 0, boxTex.width, boxTex.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0U,
            SpriteMeshType.FullRect,
            new Vector4(20, 20, 20, 20));

        RoundedBox = new LoadableAssetWrapper<Sprite>(boxSprite);
    }

    /// <summary>
    /// Gets the <see cref="Color32"/> used for teal highlighting in UI.
    /// </summary>
    public static Color32 AcceptedTeal { get; } = new(43, 233, 198, 255);

    /// <summary>
    /// Gets the Mira API <see cref="AssetBundle"/>.
    /// </summary>
    public static AssetBundle MiraAssetBundle { get; } = AssetBundleManager.Load(Assembly, "mirabundle");

    /// <summary>
    /// Gets the ModifierDisplay prefab.
    /// </summary>
    public static LoadableAsset<GameObject> ModifierDisplay { get; } = new LoadableBundleAsset<GameObject>("Modifiers", MiraAssetBundle);

    /// <summary>
    /// Gets the Popup prefab for saving presets.
    /// </summary>
    public static LoadableAsset<GameObject> PresetSavePopup { get; } = new LoadableBundleAsset<GameObject>("PresetSavePopup", MiraAssetBundle);

    /// <summary>
    /// Gets the Refresh Icon <see cref="Sprite"/>.
    /// </summary>
    public static LoadableAsset<Sprite> RefreshIcon { get; } = new LoadableBundleAsset<Sprite>("refresh", MiraAssetBundle);

    /// <summary>
    /// Gets the Folder Icon <see cref="Sprite"/>.
    /// </summary>
    public static LoadableAsset<Sprite> FolderIcon { get; } = new LoadableBundleAsset<Sprite>("freePlay_folderTaskRoom", MiraAssetBundle);

    /// <summary>
    /// Gets the empty <see cref="Sprite"/> asset.
    /// </summary>
    public static LoadableResourceAsset Empty { get; } = new("MiraAPI.Resources.Empty.png", Assembly);

    /// <summary>
    /// Gets the RoundedBox <see cref="Sprite"/>, which is a rounded rectangle used for UI elements.
    /// </summary>
    public static LoadableAsset<Sprite> RoundedBox { get; }

    /// <summary>
    /// Gets the Next Button sprite.
    /// </summary>
    public static LoadableResourceAsset NextButton { get; } = new("MiraAPI.Resources.NextButton.png", Assembly);

    /// <summary>
    /// Gets the Mira settings icon.
    /// </summary>
    public static LoadableResourceAsset SettingsIcon { get; } = new("MiraAPI.Resources.Settings.png", Assembly);

    /// <summary>
    /// Gets the highlighted Next Button sprite.
    /// </summary>
    public static LoadableResourceAsset NextButtonActive { get; } = new("MiraAPI.Resources.NextButtonActive.png", Assembly);

    /// <summary>
    /// Gets the Cog icon used in Role Settings Menu.
    /// </summary>
    public static LoadableResourceAsset Cog { get; } = new("MiraAPI.Resources.Cog.png", Assembly);

    /// <summary>
    /// Gets the Checkmark Box used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset CheckmarkBox { get; } = new("MiraAPI.Resources.CheckMarkBox.png", Assembly);

    /// <summary>
    /// Gets the Reset Box used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset ResetButton { get; } = new("MiraAPI.Resources.ResetButton.png", Assembly);

    /// <summary>
    /// Gets the Checkmark used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset Checkmark { get; } = new("MiraAPI.Resources.Checkmark.png", Assembly);

    /// <summary>
    /// Gets the white CategoryHeader used in the Settings Menu.
    /// </summary>
    public static LoadableResourceAsset CategoryHeader { get; } = new("MiraAPI.Resources.CategoryHeader.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the Keybind icons.
    /// </summary>
    public static LoadableResourceAsset KeybindButton { get; } = new("MiraAPI.Resources.KeybindButton.png", Assembly);

    /// <summary>
    /// Gets the sprite used for crewmate role files in TaskAdderGame.
    /// </summary>
    public static LoadableResourceAsset CrewmateFile { get; } = new("MiraAPI.Resources.CrewmateFile.png", Assembly);

    /// <summary>
    /// Gets the sprite used for impostor role files in TaskAdderGame.
    /// </summary>
    public static LoadableResourceAsset ImpostorFile { get; } = new("MiraAPI.Resources.ImpostorFile.png", Assembly);

    /// <summary>
    /// Gets the sprite used for custom team role files in TaskAdderGame.
    /// </summary>
    public static LoadableResourceAsset CustomTeamFile { get; } = new("MiraAPI.Resources.CustomFile.png", Assembly);

    /// <summary>
    /// Gets the sprite used for modifier file in TaskAdderGame.
    /// </summary>
    public static LoadableResourceAsset ModifierFile { get; } = new("MiraAPI.Resources.ModifierFile.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the Classic gamemode.
    /// </summary>
    public static LoadableResourceAsset ClassicGamemodeIcon { get; } = new("MiraAPI.Resources.ClassicGamemodeIcon.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the Hide n Seek gamemode.
    /// </summary>
    public static LoadableResourceAsset HnSGamemodeIcon { get; } = new("MiraAPI.Resources.HnSGamemodeIcon.png", Assembly);

    /// <summary>
    /// Gets the sprite used for timed modifier file in TaskAdderGame.
    /// </summary>
    public static LoadableResourceAsset TimedModifierFile { get; } = new("MiraAPI.Resources.TimedModifierFile.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the new chat button while hovering.
    /// </summary>
    public static LoadableResourceAsset ChatHoverSprite { get; } = new("MiraAPI.Resources.NormalChatHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the new chat button while hovering.
    /// </summary>
    public static LoadableResourceAsset ChatIdleSprite { get; } = new("MiraAPI.Resources.NormalChatIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the new chat button while hovering.
    /// </summary>
    public static LoadableResourceAsset ChatOpenSprite { get; } = new("MiraAPI.Resources.NormalChatOpen.png", Assembly);

    /// <summary>
    /// Gets the sprite used for players button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiPlayersButtonHoverSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarPlayersButtonHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the players button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiPlayersButtonIdleSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarPlayersButtonIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the players button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiPlayersButtonOpenSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarPlayersButtonOpen.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the settings button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiSettingsButtonHoverSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarSettingsButtonHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the settings button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiSettingsButtonIdleSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarSettingsButtonIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the settings button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiSettingsButtonOpenSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarSettingsButtonOpen.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiRolesButtonHoverSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarRolesButtonHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiRolesButtonIdleSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarRolesButtonIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiRolesButtonOpenSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarRolesButtonOpen.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the modifiers button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiModifiersButtonHoverSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarModifiersButtonHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the modifiers button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiModifiersButtonIdleSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarModifiersButtonIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the modifiers button while hovering.
    /// </summary>
    public static LoadableResourceAsset WikiModifiersButtonOpenSprite { get; } = new("MiraAPI.Resources.Wiki.SidebarModifiersButtonOpen.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset GroupCountHoverSprite { get; } = new("MiraAPI.Resources.Wiki.GroupCountHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset GroupCountIdleSprite { get; } = new("MiraAPI.Resources.Wiki.GroupCountIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset GroupFactionHoverSprite { get; } = new("MiraAPI.Resources.Wiki.GroupFactionHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset GroupFactionIdleSprite { get; } = new("MiraAPI.Resources.Wiki.GroupFactionIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset GroupNoneHoverSprite { get; } = new("MiraAPI.Resources.Wiki.GroupNoneHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset GroupNoneIdleSprite { get; } = new("MiraAPI.Resources.Wiki.GroupNoneIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset SearchClearHoverSprite { get; } = new("MiraAPI.Resources.Wiki.SearchClearHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset SearchClearIdleSprite { get; } = new("MiraAPI.Resources.Wiki.SearchClearIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset SearchIconHoverSprite { get; } = new("MiraAPI.Resources.Wiki.SearchIconHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset SearchIconIdleSprite { get; } = new("MiraAPI.Resources.Wiki.SearchIconIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset SortingAzHoverSprite { get; } = new("MiraAPI.Resources.Wiki.SortingAzHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset SortingAzIdleSprite { get; } = new("MiraAPI.Resources.Wiki.SortingAzIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset SortingZaHoverSprite { get; } = new("MiraAPI.Resources.Wiki.SortingZaHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset SortingZaIdleSprite { get; } = new("MiraAPI.Resources.Wiki.SortingZaIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset VisibleAllHoverSprite { get; } = new("MiraAPI.Resources.Wiki.VisibleAllHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset VisibleAllIdleSprite { get; } = new("MiraAPI.Resources.Wiki.VisibleAllIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset VisibleOffHoverSprite { get; } = new("MiraAPI.Resources.Wiki.VisibleOffHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset VisibleOffIdleSprite { get; } = new("MiraAPI.Resources.Wiki.VisibleOffIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset VisibleOnHoverSprite { get; } = new("MiraAPI.Resources.Wiki.VisibleOnHover.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the roles button while hovering.
    /// </summary>
    public static LoadableResourceAsset VisibleOnIdleSprite { get; } = new("MiraAPI.Resources.Wiki.VisibleOnIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used for crewmates.
    /// </summary>
    public static LoadableResourceAsset IconCrewmate { get; } = new("MiraAPI.Resources.Wiki.IconCrewmate.png", Assembly);

    /// <summary>
    /// Gets the sprite used for impostors.
    /// </summary>
    public static LoadableResourceAsset IconImpostor { get; } = new("MiraAPI.Resources.Wiki.IconImpostor.png", Assembly);

    /// <summary>
    /// Gets the sprite used for neutrals.
    /// </summary>
    public static LoadableResourceAsset IconNeutral { get; } = new("MiraAPI.Resources.Wiki.IconNeutral.png", Assembly);

    /// <summary>
    /// Gets the sprite used for the player list.
    /// </summary>
    public static LoadableResourceAsset IconPlayerList { get; } = new("MiraAPI.Resources.Wiki.IconPlayerList.png", Assembly);

    /// <summary>
    /// Gets the sprite used for settings.
    /// </summary>
    public static LoadableResourceAsset IconSettings { get; } = new("MiraAPI.Resources.Wiki.IconSettings.png", Assembly);

    /// <summary>
    /// Gets the sprite used for terminology.
    /// </summary>
    public static LoadableResourceAsset IconTerminology { get; } = new("MiraAPI.Resources.Wiki.IconTerminology.png", Assembly);

    /// <summary>
    /// Gets the Page switching sprite.
    /// </summary>
    public static LoadableResourceAsset PageLeftHover { get; } = new("MiraAPI.Resources.Wiki.PageLeftHover.png", Assembly);

    /// <summary>
    /// Gets the Page switching sprite.
    /// </summary>
    public static LoadableResourceAsset PageLeftIdle { get; } = new("MiraAPI.Resources.Wiki.PageLeftIdle.png", Assembly);

    /// <summary>
    /// Gets the Page switching sprite.
    /// </summary>
    public static LoadableResourceAsset PageRightHover { get; } = new("MiraAPI.Resources.Wiki.PageRightHover.png", Assembly);

    /// <summary>
    /// Gets the Page switching sprite.
    /// </summary>
    public static LoadableResourceAsset PageRightIdle { get; } = new("MiraAPI.Resources.Wiki.PageRightIdle.png", Assembly);

    /// <summary>
    /// Gets the sprite used below the tab icons in the wiki.
    /// </summary>
    public static LoadableResourceAsset TabButtonContainer { get; } = new("MiraAPI.Resources.Wiki.TabButtonContainer.png", Assembly);

    internal static LoadableResourceAsset BlankSprite { get; } = new("MiraAPI.Resources.BlankSprite.png", Assembly);
}
