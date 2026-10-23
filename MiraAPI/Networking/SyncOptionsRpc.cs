using Hazel;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace MiraAPI.Networking;

// METHOD RPC DOESN'T WORK WITH THE ARRAYS AND STUFF SO THIS IS HOW WE WILL DO IT FOR NOW
[RegisterCustomRpc((uint)MiraRpc.SyncGameOptions)]
#pragma warning disable SA1515 // Single-line comment should be preceded by blank line (Justification: ReSharper suppression.)
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated by Reactor.)
internal sealed class SyncOptionsRpc(MiraApiPlugin plugin, uint id) : PlayerCustomRpc<MiraApiPlugin, MiraOptionNetData>(plugin, id)
#pragma warning restore SA1515 // Single-line comment should be preceded by blank line
{
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

    public override void Write(MessageWriter writer, MiraOptionNetData data)
    {
        /*if (data == null)
        {
            writer.WritePacked(0U);
            return;
        }*/

        var info = data.Data;
        writer.Write(data.ShouldShowNotifications);
        writer.WritePacked((uint)info.Length);
        foreach (var netData in info)
        {
            writer.WritePacked(netData.Id);
            writer.WriteBytesAndSize(netData.Data);
        }
    }

    public override MiraOptionNetData Read(MessageReader reader)
    {
        var shouldShowNotif = reader.ReadBoolean();
        var length = reader.ReadPackedUInt32();
        var data = new NetData[length];
        for (var i = 0; i < length; i++)
        {
            var dataId = reader.ReadPackedUInt32();
            var bytes = reader.ReadBytesAndSize();
            data[i] = new NetData(dataId, bytes);
        }

        return new MiraOptionNetData(shouldShowNotif, data);
    }

    public override void Handle(PlayerControl playerControl, MiraOptionNetData data)
    {
        if (AmongUsClient.Instance.HostId != playerControl.OwnerId)
        {
            return;
        }

        ModdedOptionsManager.HandleSyncOptions(data.ShouldShowNotifications, data.Data ?? []);
    }
}
