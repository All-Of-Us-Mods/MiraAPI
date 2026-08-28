using System.Reflection;
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.MeetingAbilities;
using MiraAPI.Utilities;

namespace MiraAPI.Patches.Events;

[HarmonyPatch]
public static class FreeplayRoundStartPatch
{
    public static MethodBase TargetMethod()
    {
        return Helpers.GetStateMachineMoveNext<TutorialManager>(nameof(TutorialManager.RunTutorial))!;
    }

    public static void Postfix(ref bool __result)
    {
        if (!__result)
        {
            MeetingButtonManager.OnGameStart();
            MiraEventManager.InvokeEvent(new RoundStartEvent(true));
        }
    }
}
