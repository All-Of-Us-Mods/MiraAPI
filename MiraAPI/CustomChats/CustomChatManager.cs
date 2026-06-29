using System;
using System.Collections.Generic;
using MiraAPI.PluginLoading;

namespace MiraAPI.CustomChats;

/// <summary>
/// Manages custom chats.
/// </summary>
public static class CustomChatManager
{
    public static readonly MixedChat MixedChat = new MixedChat();
    /// <summary>
    /// Gets a list of all registered <see cref="AbstractCustomChat"/>s.
    /// </summary>
    public static readonly List<AbstractCustomChat> Chats = [new DefaultChat(), MixedChat];

    internal static bool RegisterCustomChat(Type type, MiraPluginInfo info)
    {
        if (!typeof(AbstractCustomChat).IsAssignableFrom(type))
        {
            return false;
        }

        AbstractCustomChat? chat = Activator.CreateInstance(type) as AbstractCustomChat;
        if (chat == null) return false;
        Chats.Add(chat);
        info.InternalChats.Add(chat);
        return true;
    }
}
