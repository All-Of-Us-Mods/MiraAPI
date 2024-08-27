using HarmonyLib;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(TaskPanelBehaviour))]
public static class TaskPanelPatch
{
    /// <summary>
    /// This patch is to override the automatic updating of the y position on the tab (which is in base game), 
    /// because I can't change the custom tab y pos if it's being overriden every frame.
    /// Im sure there is an easier/better way, but this is the fix that worked for me
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(TaskPanelBehaviour.Update))]
    public static bool UpdatePrefix(TaskPanelBehaviour __instance)
    {
        if (__instance.gameObject.name != "RolePanel")
        {
            return true;
        }

        var transform = __instance.background.transform;
        var vector = __instance.background.sprite.bounds.extents;
        var vector2 = __instance.tab.sprite.bounds.extents;

        transform.localScale = __instance.taskText.textBounds.size.x > 0f ? new Vector3(__instance.taskText.textBounds.size.x + 0.4f, __instance.taskText.textBounds.size.y + 0.3f, 1f) : Vector3.zero;

        vector.y = -vector.y;
        vector = vector.Mul(transform.localScale);
        __instance.background.transform.localPosition = vector;

        vector2 = vector2.Mul(__instance.tab.transform.localScale);
        vector2.y = -vector2.y;
        vector2.x += vector.x * 2f;
        __instance.tab.transform.localPosition = vector2;

        if (!GameManager.Instance)
        {
            return false;
        }
        var closePosition = new Vector3(-__instance.background.sprite.bounds.size.x * __instance.background.transform.localScale.x, __instance.closedPosition.y, __instance.closedPosition.z);
        __instance.closedPosition = closePosition;
        if (__instance.open)
        {
            __instance.timer = Mathf.Min(1f, __instance.timer + Time.deltaTime / __instance.animationTimeSeconds);
        }
        else
        {
            __instance.timer = Mathf.Max(0f, __instance.timer - Time.deltaTime / __instance.animationTimeSeconds);
        }
        Vector3 relativePos;
        relativePos = new Vector3(Mathf.SmoothStep(__instance.closedPosition.x, __instance.openPosition.x, __instance.timer), Mathf.SmoothStep(__instance.closedPosition.y, __instance.openPosition.y, __instance.timer), __instance.openPosition.z);
        __instance.transform.localPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop, relativePos);
        return false;
    }

}