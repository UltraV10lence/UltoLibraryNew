using UltoLibraryNew.Network.Packets;

namespace UltoLibraryNew.Network.ConnectionInitializers;

public interface ServerConnectionInitializer : ConnectionInitializer {
    public string RemoteIp { get; }
    public ushort RemotePort { get; }
    public new ServerConnectionInitializer RegisterPackets(Action<PacketTypeIdentifier> identifier);
}