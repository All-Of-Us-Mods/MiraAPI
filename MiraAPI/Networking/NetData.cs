namespace MiraAPI.Networking;

public readonly struct NetData(uint id, byte[] data)
{
    public uint Id { get; } = id;
    public byte[] Data { get; } = data;
    public int GetLength() => 4 + Data.Length;
}