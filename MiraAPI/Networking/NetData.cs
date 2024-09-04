namespace MiraAPI.Networking;

/// <summary>
/// Used to network data and mark it with an ID.
/// </summary>
/// <param name="id">The ID of the data.</param>
/// <param name="data">The byte[] data.</param>
public readonly struct NetData(uint id, byte[] data)
{
    /// <summary>
    /// Gets the ID of the data.
    /// </summary>
    public uint Id { get; } = id;

    /// <summary>
    /// Gets the byte[] data.
    /// </summary>
    public byte[] Data { get; } = data;

    /// <summary>
    /// Gets the length of the data in bytes.
    /// </summary>
    /// <returns>An int representing the number of bytes this NetData takes up.</returns>
    public int GetLength() => 4 + Data.Length;
}
