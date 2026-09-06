using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;

namespace MiraAPI.VanillaEvents;

internal static class UiResetEvents
{
    public static void Initialize()
    {
        MiraEventManager.RegisterEventHandler<UiButtonResetEvent>(ResetButtonParents);
        MiraEventManager.RegisterEventHandler<UiButtonPostResetEvent>(PlaceWikiButton, -900);
        MiraEventManager.RegisterEventHandler<UiButtonPostResetEvent>(PlaceSubmergedButton, -800);
        MiraEventManager.RegisterEventHandler<UiButtonPostResetEvent>(PlaceModifierUi, -700);
    }

    private static void ResetButtonParents(UiButtonResetEvent _)
    {
        var wikiButton = MiraHudHelper.VanillaMatchInfoButton;
        var subButton = MiraHudHelper.SubmergedFloorButton;
        var modDisplay = MiraHudHelper.ModifierDisplayOnRight ? MiraHudHelper.ModifierDisplayObject : null!;
        if (wikiButton)
        {
            wikiButton.transform.SetParent(null);
        }
        if (subButton)
        {
            subButton.transform.SetParent(null);
        }
        if (modDisplay)
        {
            modDisplay.transform.SetParent(null);
        }
    }

    private static void PlaceWikiButton(UiButtonPostResetEvent @event)
    {
        var wikiButton = MiraHudHelper.VanillaMatchInfoButton;
        if (!wikiButton)
        {
            return;
        }
        var firstRow = @event.MainTopUiRow;
        var secondRow = @event.SecondTopUiRow;
        var opts = LocalSettingsTabSingleton<MiraApiSettings>.Instance;
        wikiButton.transform.SetParent(opts.WikiOnBottomRow.Value ? secondRow.transform : firstRow.transform);
    }

    private static void PlaceSubmergedButton(UiButtonPostResetEvent @event)
    {
        var subButton = MiraHudHelper.SubmergedFloorButton;
        if (!subButton)
        {
            return;
        }
        var secondRow = @event.SecondTopUiRow;
        subButton.transform.SetParent(secondRow.transform);
    }

    private static void PlaceModifierUi(UiButtonPostResetEvent @event)
    {
        var modDisplay = MiraHudHelper.ModifierDisplayOnRight ? MiraHudHelper.ModifierDisplayObject : null!;
        if (!modDisplay)
        {
            return;
        }
        var secondRow = @event.SecondTopUiRow;
        modDisplay.transform.SetParent(secondRow.transform);
    }
}
