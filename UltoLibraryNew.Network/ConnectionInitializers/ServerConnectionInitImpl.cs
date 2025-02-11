using UltoLibraryNew.Network.Packets;

namespace UltoLibraryNew.Network.ConnectionInitializers;

internal class ServerConnectionInitImpl : ServerConnectionInitializer {
    public readonly TcpServer Server;
    public readonly TcpConnection Connection;

    public string RemoteIp => Connection.RemoteIp;
    public ushort RemotePort => Connection.RemotePort;
    
    public ServerConnectionInitImpl(TcpConnection connection, TcpServer server) {
        Connection = connection;
        Server = server;
    }
    
    public ServerConnectionInitializer RegisterPackets(Action<PacketTypeIdentifier> identifier) {
        identifier.Invoke(Connection.PacketIdentifier);
        return this;
    }

    ConnectionInitializer ConnectionInitializer.RegisterPackets(Action<PacketTypeIdentifier> identifier) {
        return RegisterPackets(identifier);
    }
}