using System;
using Hazel;
using InnerNet;
using MiraAPI.CustomChats;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using Reactor.Networking.Serialization;

namespace MiraAPI.Networking;

/// <summary>
/// Converter for serializing and deserializing <see cref="AbstractCustomChat"/> objects.
/// </summary>
[MessageConverter]
public class CustomChatConverter : MessageConverter<AbstractCustomChat>
{
    /// <summary>
    /// Writes a <see cref="AbstractCustomChat"/> to the writer.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The <see cref="AbstractCustomChat"/> to write.</param>
    public override void Write(MessageWriter writer, AbstractCustomChat value)
    {
        writer.Write(CustomChatManager.Chats.IndexOf(value));
    }

    /// <summary>
    /// Reads a <see cref="AbstractCustomChat"/> from the reader.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="objectType">The type of the object to read.</param>
    /// <returns>The <see cref="AbstractCustomChat"/> that was read.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the custom chat is not found.</exception>
    public override AbstractCustomChat Read(MessageReader reader, Type objectType)
    {
        return CustomChatManager.Chats[reader.ReadInt32()];
    }
}
