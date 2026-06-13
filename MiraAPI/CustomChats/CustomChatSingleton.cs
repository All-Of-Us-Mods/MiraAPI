using System.Linq;

namespace MiraAPI.CustomChats;

/// <summary>
/// Singleton for accessing custom chats.
/// </summary>
/// <typeparam name="T">The custom chat type.</typeparam>
public static class CustomChatSingleton<T> where T : AbstractCustomChat
{
    private static T? _instance;

    /// <summary>
    /// Gets the instance of the option group.
    /// </summary>
#pragma warning disable CA1000
    public static T Instance => _instance ??= CustomChatManager.Chats.OfType<T>().Single();
#pragma warning restore CA1000
}
