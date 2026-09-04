using MiraAPI.Events;
using MiraAPI.Events.Mira;
using MiraAPI.Hud;
using MiraAPI.LocalSettings;

namespace MiraAPI.VanillaEvents;

public static class UiResetEvents
{
    public static void Initialize()
    {
        MiraEventManager.RegisterEventHandler<UiButtonResetEvent>(@event => ResetButtonParents(@event));
        MiraEventManager.RegisterEventHandler<UiButtonPostResetEvent>(@event => PlaceWikiButton(@event), -900);
        MiraEventManager.RegisterEventHandler<UiButtonPostResetEvent>(@event => PlaceSubmergedButton(@event), -800);
        MiraEventManager.RegisterEventHandler<UiButtonPostResetEvent>(@event => PlaceModifierUi(@event), -700);
    }

    public static void ResetButtonParents(UiButtonResetEvent @event)
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

    public static void PlaceWikiButton(UiButtonPostResetEvent @event)
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

    public static void PlaceSubmergedButton(UiButtonPostResetEvent @event)
    {
        var subButton = MiraHudHelper.SubmergedFloorButton;
        if (!subButton)
        {
            return;
        }
        var secondRow = @event.SecondTopUiRow;
        subButton.transform.SetParent(secondRow.transform);
    }

    public static void PlaceModifierUi(UiButtonPostResetEvent @event)
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
