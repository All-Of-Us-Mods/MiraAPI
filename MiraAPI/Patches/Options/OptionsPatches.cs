using HarmonyLib;
using Il2CppSystem;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using MiraAPI.Translation;
using MiraAPI.Utilities;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch]
public static class OptionsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.IncreaseChance))]
    public static bool RoleIncreaseChancePrefix(RoleOptionSetting __instance)
    {
        if (!__instance.Role.IsCustomRole())
        {
            return true;
        }

        if (__instance.roleChance == 0)
        {
            __instance.roleMaxCount = 1;
        }

        var increment = Input.GetKey(KeyCode.LeftShift) ? 5 : 10;
        __instance.roleChance += increment;

        if (__instance.roleChance > 100)
        {
            __instance.roleChance = 0;
        }

        __instance.OnValueChanged.Invoke(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.DecreaseChance))]
    public static bool RoleDecreaseChancePrefix(RoleOptionSetting __instance)
    {
        if (!__instance.Role.IsCustomRole())
        {
            return true;
        }

        if (__instance.roleChance == 0)
        {
            __instance.roleMaxCount = 1;
        }

        var increment = Input.GetKey(KeyCode.LeftShift) ? 5 : 10;
        __instance.roleChance -= increment;

        if (__instance.roleChance < 0)
        {
            __instance.roleChance = 100;
        }

        __instance.OnValueChanged.Invoke(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.IncreaseCount))]
    public static bool RoleIncreaseCountPrefix(RoleOptionSetting __instance)
    {
        if (!__instance.Role.IsCustomRole())
        {
            return true;
        }

        if (__instance.roleMaxCount == 0)
        {
            __instance.roleChance = 50;
        }

        __instance.roleMaxCount += 1;
        if (__instance.roleMaxCount > __instance.role.MaxCount)
        {
            __instance.roleMaxCount = 0;
        }

        __instance.OnValueChanged.Invoke(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.DecreaseCount))]
    public static bool RoleDecreaseCountPrefix(RoleOptionSetting __instance)
    {
        if (!__instance.Role.IsCustomRole())
        {
            return true;
        }

        if (__instance.roleMaxCount == 0)
        {
            __instance.roleChance = 50;
        }

        __instance.roleMaxCount -= 1;
        if (__instance.roleMaxCount < 0)
        {
            __instance.roleMaxCount = __instance.role.MaxCount;
        }

        __instance.OnValueChanged.Invoke(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.AdjustButtonsActiveState))]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.AdjustButtonsActiveState))]
    [HarmonyPatch(typeof(PlayerOption), nameof(PlayerOption.AdjustButtonsActiveState))]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.AdjustChanceButtonsActiveState))]
    [HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.AdjustCountButtonsActiveState))]
    public static bool AdjustButtonsPrefix(OptionBehaviour __instance)
    {
        if (__instance.IsCustom())
        {
            if (__instance.TryCast<NumberOption>() is { } numberOption)
            {
                numberOption.MinusBtn.SetInteractable(true);
                numberOption.PlusBtn.SetInteractable(true);
            }
            if (__instance.TryCast<StringOption>() is { } stringOption)
            {
                stringOption.MinusBtn.SetInteractable(true);
                stringOption.PlusBtn.SetInteractable(true);
            }
            if (__instance.TryCast<PlayerOption>() is { } playerOption)
            {
                playerOption.MinusBtn.SetInteractable(true);
                playerOption.PlusBtn.SetInteractable(true);
            }

            return false;
        }

        if (__instance.TryCast<RoleOptionSetting>() is { } roleOptionSetting && roleOptionSetting.Role.IsCustomRole())
        {
            roleOptionSetting.CountMinusBtn.SetInteractable(true);
            roleOptionSetting.CountPlusBtn.SetInteractable(true);
            roleOptionSetting.ChanceMinusBtn.SetInteractable(true);
            roleOptionSetting.ChancePlusBtn.SetInteractable(true);

            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Initialize))]
    public static bool ToggleInit(ToggleOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.TitleText.text = TranslationController.Instance.GetString(__instance.Title, Array.Empty<Object>()).Translate();

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Toggle))]
    // UpdateValue was inlined.
    public static bool ToggleUpdate(ToggleOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.CheckMark.enabled = !__instance.CheckMark.enabled;
        __instance.OnValueChanged.Invoke(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.UpdateValue))]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
    [HarmonyPatch(typeof(PlayerOption), nameof(PlayerOption.UpdateValue))]
    public static bool UpdateValuePrefix(OptionBehaviour __instance)
    {
        return !__instance.IsCustom();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
    public static bool NumberInit(NumberOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }
        __instance.TitleText.text = TranslationController.Instance.GetString(__instance.Title).Translate();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Increase))]
    public static bool NumberIncrease(NumberOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        var increment = __instance.Increment;

        if (Input.GetKey(KeyCode.LeftShift) &&
            __instance.TryGetComponent<MiraNumberOptionComponent>(out var miraNumber) &&
            miraNumber.ShiftIncrementToggle)
        {
            increment /= 2;
        }

        __instance.Value += increment;
        if (__instance.Value > __instance.ValidRange.max)
        {
            __instance.Value = __instance.ValidRange.min;
        }
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Decrease))]
    public static bool NumberDecrease(NumberOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        var increment = __instance.Increment;

        if (Input.GetKey(KeyCode.LeftShift) &&
            __instance.TryGetComponent<MiraNumberOptionComponent>(out var miraNumber) &&
            miraNumber.ShiftIncrementToggle)
        {
            increment /= 2;
        }

        __instance.Value -= increment;
        if (__instance.Value < __instance.ValidRange.min)
        {
            __instance.Value = __instance.ValidRange.max;
        }
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Increase))]
    public static bool StringOptionIncrease(StringOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.Value += 1;
        if (__instance.Value >= __instance.Values.Length)
        {
            __instance.Value = 0;
        }
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Decrease))]
    public static bool StringOptionDecrease(StringOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.Value -= 1;
        if (__instance.Value < 0)
        {
            __instance.Value = __instance.Values.Length - 1;
        }
        __instance.OnValueChanged.Invoke(__instance);
        __instance.AdjustButtonsActiveState();
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
    public static bool StringInit(StringOption __instance)
    {
        if (!__instance.IsCustom())
        {
            return true;
        }

        __instance.TitleText.text = TranslationController.Instance.GetString(__instance.Title, Array.Empty<Object>()).Translate();
        __instance.ValueText.text = TranslationController.Instance.GetString(__instance.Values[__instance.Value], Array.Empty<Object>());

        return false;
    }
}
