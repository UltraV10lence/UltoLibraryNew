using UltoLibraryNew.Network.Packets;

namespace UltoLibraryNew.Network.ConnectionInitializers;

internal class ConnectionInitImpl : ConnectionInitializer {
    public readonly TcpClient Client;
    public readonly TcpConnection Connection;
    
    public ConnectionInitImpl(TcpConnection connection, TcpClient client) {
        Connection = connection;
        Client = client;
    }

    public ConnectionInitializer RegisterPackets(Action<PacketTypeIdentifier> identifier) {
        identifier.Invoke(Connection.PacketIdentifier);
        return this;
    }
}