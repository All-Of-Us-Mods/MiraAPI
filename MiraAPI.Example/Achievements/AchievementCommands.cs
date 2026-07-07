using System.Globalization;
using HarmonyLib;
using MiraAPI.Achievements;

namespace MiraAPI.Example.Achievements;

[HarmonyPatch]
public static class AchievementCommands
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    private static bool SendChatPrefix(ChatController __instance)
    {
        var text = __instance.freeChatField.textArea.text.Trim();
        if (!text.StartsWith("/achievement", true, CultureInfo.InvariantCulture))
        {
            return true;
        }

        var parts = text.Split(' ');
        if (parts.Length < 2)
        {
            __instance.AddChat(PlayerControl.LocalPlayer, "Usage: /achievement <id>  |  /achievement unlock <id>");
            __instance.freeChatField.textArea.Clear();
            return false;
        }

        if (parts[1].Equals("unlock", System.StringComparison.OrdinalIgnoreCase))
        {
            if (parts.Length < 3)
            {
                __instance.AddChat(PlayerControl.LocalPlayer, "Usage: /achievement unlock <id>");
                __instance.freeChatField.textArea.Clear();
                return false;
            }

            var unlockId = parts[2];
            if (!MiraAPI.Achievements.AchievementManager.TryGetAchievement(unlockId, out var unlockAch))
            {
                __instance.AddChat(PlayerControl.LocalPlayer, $"Achievement '{unlockId}' not found.");
                __instance.freeChatField.textArea.Clear();
                return false;
            }

            unlockAch.SetProgress(PlayerControl.LocalPlayer, unlockAch.Goal);
            __instance.AddChat(PlayerControl.LocalPlayer, $"Force unlocked: [{unlockAch.Title}]");
            __instance.freeChatField.textArea.Clear();
            return false;
        }

        var achievementId = parts[1];
        if (!MiraAPI.Achievements.AchievementManager.TryGetAchievement(achievementId, out var achievement))
        {
            __instance.AddChat(PlayerControl.LocalPlayer, $"Achievement '{achievementId}' not found.");
            __instance.freeChatField.textArea.Clear();
            return false;
        }

        if (!achievement.IsUnlocked(PlayerControl.LocalPlayer))
        {
            __instance.AddChat(PlayerControl.LocalPlayer, $"You haven't unlocked '{achievement.Title}' yet. Use /achievement unlock {achievementId}");
            __instance.freeChatField.textArea.Clear();
            return false;
        }

        MiraAPI.Achievements.AchievementManager.SetEquippedTitle(PlayerControl.LocalPlayer, achievementId);
        TitleSyncRpc.Send(PlayerControl.LocalPlayer.PlayerId, achievementId);
        __instance.AddChat(PlayerControl.LocalPlayer, $"Title equipped: [{achievement.Title}]");
        __instance.freeChatField.textArea.Clear();
        return false;
    }
}
