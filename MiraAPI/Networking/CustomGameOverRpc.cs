using System.Collections.Generic;
using Hazel;
using InnerNet;
using MiraAPI.GameEnd;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace MiraAPI.Networking;

/// <summary>
/// Custom Game Over RPC.
/// </summary>
/// <inheritdoc />
[RegisterCustomRpc((uint)MiraRpc.CustomGameOver)]
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated via Activator.CreateInstance.)
public class CustomGameOverRpc(MiraApiPlugin plugin, uint id) : PlayerCustomRpc<MiraApiPlugin, GameOverData>(plugin, id)
{
    /// <inheritdoc />
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

    /// <inheritdoc />
    public override void Write(MessageWriter writer, GameOverData data)
    {
        writer.WritePacked(data.Reason);
        writer.WritePacked(data.Winners.Count);
        foreach (var player in data.Winners)
        {
            writer.WriteNetObject(player);
        }
    }

    /// <inheritdoc />
    public override GameOverData Read(MessageReader reader)
    {
        var reason = reader.ReadPackedInt32();
        var count = reader.ReadPackedInt32();
        var data = new List<NetworkedPlayerInfo>(count);
        for (var i = 0; i < count; i++)
        {
            data.Add(reader.ReadNetObject<NetworkedPlayerInfo>());
        }

        return new GameOverData(reason, data);
    }

    /// <inheritdoc />
    public override void Handle(PlayerControl innerNetObject, GameOverData data)
    {
        if (GameOverManager.TryGetGameOver(data.Reason, out var gameOver))
        {
            if (!gameOver.VerifyCondition(innerNetObject, [.. data.Winners]))
            {
                Info($"Game over condition not met for {gameOver.GetType().Name}");
                return;
            }

            CustomGameOver.Instance = gameOver;

            if (AmongUsClient.Instance.AmHost)
            {
                GameManager.Instance.RpcEndGame(gameOver, false);
            }
        }
        else
        {
            CustomGameOver.Instance = null;
            Error($"Unknown game over reason: {data.Reason}");
        }
    }
}
