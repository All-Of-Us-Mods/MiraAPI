using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Utilities;
using UnityEngine;

namespace MiraAPI.Patches.Options;

[HarmonyPatch]
public static class OptionsPatches
{
    private static bool HandleRoleChance(RoleOptionSetting instance, int direction)
    {
        if (!instance.Role.IsCustomRole())
            return true;

        if (instance.roleChance == 0)
            instance.roleMaxCount = 1;

        var increment = (Input.GetKey(KeyCode.LeftShift) ? 5 : 10) * direction;
        instance.roleChance += increment;

        if (instance.roleChance > 100)
            instance.roleChance = 0;
        else if (instance.roleChance < 0)
            instance.roleChance = 100;

        if (instance.roleChance == 0)
            instance.roleMaxCount = 0;

        instance.OnValueChanged.Invoke(instance);
        return false;
    }

    private static bool HandleRoleCount(RoleOptionSetting instance, int direction)
    {
        if (!instance.Role.IsCustomRole())
            return true;

        if (instance.roleMaxCount == 0)
            instance.roleChance = 50;

        instance.roleMaxCount += direction;

        if (instance.roleMaxCount > instance.role.MaxCount)
            instance.roleMaxCount = 0;
        else if (instance.roleMaxCount < 0)
            instance.roleMaxCount = instance.role.MaxCount;

        if (instance.roleMaxCount == 0)
            instance.roleChance = 0;

        instance.OnValueChanged.Invoke(instance);
        return false;
    }

    private static bool HandleNumberOption(NumberOption instance, int direction)
    {
        if (!instance.IsCustom())
            return true;

        var increment = instance.Increment;

        if (Input.GetKey(KeyCode.LeftShift) &&
            instance.TryGetComponent<MiraNumberOptionComponent>(out var miraNumber) &&
            miraNumber.ShiftIncrementToggle)
        {
            increment /= 2;
        }

        instance.Value += increment * direction;

        if (instance.Value > instance.ValidRange.max)
            instance.Value = instance.ValidRange.min;
        else if (instance.Value < instance.ValidRange.min)
            instance.Value = instance.ValidRange.max;

        instance.OnValueChanged.Invoke(instance);
        instance.AdjustButtonsActiveState();
        return false;
    }

    private static bool HandleStringOption(StringOption instance, int direction)
    {
        if (!instance.IsCustom())
            return true;

        instance.Value += direction;

        if (instance.Value >= instance.Values.Length)
            instance.Value = 0;
        else if (instance.Value < 0)
            instance.Value = instance.Values.Length - 1;

        instance.OnValueChanged.Invoke(instance);
        instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.IncreaseChance))]
    public static bool RoleIncreaseChancePrefix(RoleOptionSetting __instance) => HandleRoleChance(__instance, 1);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.DecreaseChance))]
    public static bool RoleDecreaseChancePrefix(RoleOptionSetting __instance) => HandleRoleChance(__instance, -1);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.IncreaseCount))]
    public static bool RoleIncreaseCountPrefix(RoleOptionSetting __instance) => HandleRoleCount(__instance, 1);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.DecreaseCount))]
    public static bool RoleDecreaseCountPrefix(RoleOptionSetting __instance) => HandleRoleCount(__instance, -1);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
    public static bool NumberIncrease(NumberOption __instance) => HandleNumberOption(__instance, 1);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
    public static bool NumberDecrease(NumberOption __instance) => HandleNumberOption(__instance, -1);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public static bool StringOptionIncrease(StringOption __instance) => HandleStringOption(__instance, 1);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public static bool StringOptionDecrease(StringOption __instance) => HandleStringOption(__instance, -1);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.AdjustButtonsActiveState))]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.AdjustButtonsActiveState))]
    [HarmonyPatch(typeof(PlayerOption), nameof(PlayerOption.AdjustButtonsActiveState))]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.AdjustChanceButtonsActiveState))]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.AdjustCountButtonsActiveState))]
    public static bool AdjustButtonsPrefix(OptionBehaviour __instance)
    {
        if (!__instance.IsCustom())
        {
            if (!__instance.TryCast<RoleOptionSetting>(out var roleOptionSetting) || !roleOptionSetting.Role.IsCustomRole())
                return true;

            roleOptionSetting.CountMinusBtn.SetInteractable(true);
            roleOptionSetting.CountPlusBtn.SetInteractable(true);
            roleOptionSetting.ChanceMinusBtn.SetInteractable(true);
            roleOptionSetting.ChancePlusBtn.SetInteractable(true);
            return false;
        }

        if (__instance.TryCast<NumberOption>(out var numberOption))
        {
            numberOption.MinusBtn.SetInteractable(true);
            numberOption.PlusBtn.SetInteractable(true);
        }
        else if (__instance.TryCast<StringOption>(out var stringOption))
        {
            stringOption.MinusBtn.SetInteractable(true);
            stringOption.PlusBtn.SetInteractable(true);
        }
        else if (__instance.TryCast<PlayerOption>(out var playerOption))
        {
            playerOption.MinusBtn.SetInteractable(true);
            playerOption.PlusBtn.SetInteractable(true);
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
    public static bool ToggleUpdate(ToggleOption __instance)
    {
        if (!__instance.IsCustom())
            return true;

        __instance.CheckMark.enabled = !__instance.CheckMark.enabled;
        __instance.OnValueChanged.Invoke(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.UpdateValue))]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
    [HarmonyPatch(typeof(PlayerOption), nameof(PlayerOption.UpdateValue))]
    public static bool UpdateValuePrefix(OptionBehaviour __instance) => !__instance.IsCustom();
}
