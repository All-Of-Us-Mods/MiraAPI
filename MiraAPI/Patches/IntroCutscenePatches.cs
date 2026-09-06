using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameModes;
using MiraAPI.MeetingAbilities;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;

namespace MiraAPI.Patches;

[HarmonyPatch(typeof(IntroCutscene))]
public static class IntroCutscenePatches
{
    /*
    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    public static void BeginImpostorPatch(IntroCutscene __instance)
    {
        if (CustomGameModeManager.ActiveMode != null && CustomGameModeManager.ActiveMode.ShowCustomRoleScreen())
        {
            var mode = CustomGameModeManager.ActiveMode;
            __instance.TeamTitle.text = $"<size=70%>{mode.Name}</size>\n<size=20%>{mode.Description}</size>";
        }
    }*/

    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.CoBegin))]
    public static void IntroBeginPatch(IntroCutscene __instance)
    {
        var @event = new IntroBeginEvent(__instance);
        MiraEventManager.InvokeEvent(@event);

        if (CustomGameModeManager.ActiveMode != null)
        {
            CustomGameModeManager.ActiveMode.Initialize();
        }
    }

    [HarmonyPatch]
    public static class IntroCutsceneShowRolePatch
    {
        public static MethodBase TargetMethod()
        {
            return Helpers.GetStateMachineMoveNext<IntroCutscene>(nameof(IntroCutscene.ShowRole))!;
        }

        public static void Postfix(Il2CppObjectBase __instance)
        {
            var wrapper = new StateMachineWrapper<IntroCutscene>(__instance);
            // run before the first yield
            if (wrapper.GetState() != 1)
            {
                return;
            }

            var introCutscene = wrapper.Instance;

            Info("IntroCutscene ShowRole reached");
            var @event = new IntroRoleRevealEvent(introCutscene);
            MiraEventManager.InvokeEvent(@event);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
    public static bool BeginPrefix(IntroCutscene __instance, [HarmonyArgument(0)] ref Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam)
    {
        return PlayerControl.LocalPlayer.Data.Role is not ICustomRole customRole || customRole.SetupIntroTeam(__instance, ref yourTeam);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(IntroCutscene.BeginImpostor))]
    [HarmonyPatch(nameof(IntroCutscene.BeginCrewmate))]
    public static void BeginPostfix(IntroCutscene __instance)
    {
        if (PlayerControl.LocalPlayer.Data.Role is not ICustomRole customRole)
        {
            return;
        }

        if (customRole.IntroConfiguration is { } introConfig)
        {
            __instance.BackgroundBar.material.SetColor(ShaderID.Color, introConfig.IntroTeamColor);
            __instance.TeamTitle.color = introConfig.IntroTeamColor;
            __instance.TeamTitle.text = introConfig.IntroTeamTitle.Translate();
            __instance.ImpostorText.text = introConfig.IntroTeamDescription.Translate();
        }
    }

    [HarmonyPatch]
    public static class IntroCutsceneDestroyPatch
    {
        private static bool _usedFallback;

        public static MethodBase TargetMethod()
        {
            var onDestroy = AccessTools.Method(typeof(IntroCutscene), "OnDestroy");
            if (onDestroy != null)
            {
                _usedFallback = false;
                Info("Using OnDestroy for IntroCutsceneDestroyPatch");
                return onDestroy;
            }

            _usedFallback = true;
            return Helpers.GetStateMachineMoveNext<IntroCutscene>(nameof(IntroCutscene.CoBegin))!;
        }

        public static void Postfix(Il2CppObjectBase __instance)
        {
            IntroCutscene introCutscene;

            if (_usedFallback)
            {
                var wrapper = new StateMachineWrapper<IntroCutscene>(__instance);
                // run after the final yield
                if (wrapper.GetState() != -1)
                {
                    return;
                }
                introCutscene = wrapper.Instance;
            }
            else
            {
                introCutscene = __instance.Cast<IntroCutscene>();
            }
            Info("IntroCutscene ended");

            MeetingButtonManager.OnGameStart();
            MiraEventManager.InvokeEvent(new IntroEndEvent(introCutscene));

            var @event = new BeforeRoundStartEvent(true);
            MiraEventManager.InvokeEvent(@event);

            if (@event.IsCancelled) return;
            MiraEventManager.InvokeEvent(new RoundStartEvent(true));
        }
    }
}
