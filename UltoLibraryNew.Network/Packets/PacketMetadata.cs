namespace UltoLibraryNew.Network.Packets;

public struct PacketMetadata {
    public bool IsSystemPacket => DataType < 0;
    
    public byte ChannelId;
    public int PacketLength;
    public short DataType;
}