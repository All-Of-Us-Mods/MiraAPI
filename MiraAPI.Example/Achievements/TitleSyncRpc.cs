using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace MiraAPI.Example.Achievements;

[RegisterCustomRpc(100)]
public class TitleSyncRpc(ExamplePlugin plugin, uint id) : PlayerCustomRpc<ExamplePlugin, TitleSyncRpc.Data>(plugin, id)
{
    public class Data
    {
        public byte PlayerId { get; set; }
        public string AchievementId { get; set; } = "-";
    }

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

    public override void Write(MessageWriter writer, Data data)
    {
        writer.Write(data.PlayerId);
        writer.Write(data.AchievementId);
    }

    public override Data Read(MessageReader reader)
    {
        return new Data
        {
            PlayerId = reader.ReadByte(),
            AchievementId = reader.ReadString(),
        };
    }

    public override void Handle(PlayerControl innerNetObject, Data data)
    {
        MiraAPI.Achievements.AchievementManager.SetEquippedTitleById(data.PlayerId, data.AchievementId);
    }

    public static void Send(byte playerId, string? achievementId)
    {
        Rpc<TitleSyncRpc>.Instance.SendTo(PlayerControl.LocalPlayer, -1, new Data
        {
            PlayerId = playerId,
            AchievementId = achievementId ?? "-",
        });
    }
}
