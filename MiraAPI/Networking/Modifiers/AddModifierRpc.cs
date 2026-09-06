using System;
using Hazel;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Networking.Serialization;

namespace MiraAPI.Networking.Modifiers;

/// <summary>
/// Remote procedure call for adding a <see cref="BaseModifier"/>.
/// </summary>
/// <param name="plugin">Mira plugin.</param>
/// <param name="id">RPC ID.</param>
[RegisterCustomRpc((uint)MiraRpc.AddModifier)]
// ReSharper disable once ClassNeverInstantiated.Global (Justification: Instantiated by Reactor.)
public class AddModifierRpc(MiraApiPlugin plugin, uint id) : PlayerCustomRpc<MiraApiPlugin, ModifierData>(plugin, id)
{
    /// <inheritdoc />
    public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

    /// <inheritdoc />
    public override void Write(MessageWriter writer, ModifierData data)
    {
        writer.WritePacked(data.TypeId);
        writer.WriteBytesAndSize(data.UniqueId.ToByteArray());
        writer.WritePacked(data.Args.Length);
        foreach (var arg in data.Args)
        {
            writer.Write(arg.GetType().AssemblyQualifiedName);
            writer.Serialize(arg);
        }
    }

    /// <inheritdoc />
    public override ModifierData Read(MessageReader reader)
    {
        var modId = reader.ReadPackedUInt32();
        var guid = new Guid(reader.ReadBytesAndSize());
        var argCount = reader.ReadPackedUInt32();
        var objects = new object[argCount];

        if (argCount > 0)
        {
            var types = new Type[argCount];
            for (var i = 0; i < argCount; i++)
            {
                var name = reader.ReadString();
                types[i] = Type.GetType(name) ?? throw new InvalidOperationException($"Type not found: {name}");
                objects[i] = reader.Deserialize(types[i]);
            }
        }

        return new ModifierData(modId, guid, objects);
    }

    /// <inheritdoc />
    public override void Handle(PlayerControl player, ModifierData data)
    {
        var type = ModifierManager.GetModifierType(data.TypeId) ?? throw new InvalidOperationException($"Modifier type not found for ID {data.TypeId}.");
        var modifier = data.Args.Length > 0
            ? ModifierFactory.CreateInstance(type, data.Args)
            : Activator.CreateInstance(type) as BaseModifier
                ?? throw new InvalidOperationException($"Cannot add modifier {type.Name} because it is not a valid modifier.");
        modifier.UniqueId = data.UniqueId;
        player.AddModifier(modifier);
    }
}
