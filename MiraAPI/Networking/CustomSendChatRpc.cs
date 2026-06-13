using MiraAPI.CustomChats;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace MiraAPI.Networking;

/// <summary>
/// Custom SendChat RPC adapted for custom chats.
/// </summary>
public static class CustomSendChatRpc
{
    [MethodRpc((uint)MiraRpc.SendChat, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcCustomSendChat(
        this PlayerControl source,
        string chatText,
        AbstractCustomChat chat)
    {
        if (!chat.CanSee()) return;

        HudManager.Instance.Chat.CustomAddChat(source, chatText, chat);
    }
}
