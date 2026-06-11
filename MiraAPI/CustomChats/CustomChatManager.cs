using System;
using System.Collections.Generic;
using MiraAPI.PluginLoading;

namespace MiraAPI.CustomChats;

/// <summary>
/// Manages custom chats.
/// </summary>
public static class CustomChatManager
{
    /// <summary>
    /// Gets a list of all registered <see cref="CustomChat"/>s.
    /// </summary>
    public static readonly List<CustomChat> Chats = [new DefaultChat()];

    internal static bool RegisterCustomChat(Type type, MiraPluginInfo info)
    {
        if (!typeof(CustomChat).IsAssignableFrom(type))
        {
            return false;
        }

        CustomChat? chat = Activator.CreateInstance(type) as CustomChat;
        if (chat == null) return false;
        Chats.Add(chat);
        info.InternalChats.Add(chat);
        return true;
    }
}
