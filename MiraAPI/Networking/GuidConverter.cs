using System;
using Hazel;
using Reactor.Networking.Attributes;
using Reactor.Networking.Serialization;

namespace MiraAPI.Networking;

/// <summary>
/// Converter for <see cref="Guid"/>.
/// </summary>
[MessageConverter]
public class GuidConverter : MessageConverter<Guid>
{
    /// <inheritdoc/>
    public override void Write(MessageWriter writer, Guid value)
    {
        writer.WriteBytesAndSize(value.ToByteArray());
    }

    /// <inheritdoc/>
    public override Guid Read(MessageReader reader, Type objectType)
    {
        return new Guid(reader.ReadBytesAndSize());
    }
}
