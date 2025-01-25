namespace UltoLibraryNew.Network;

public class TcpChannel {
    public string Identifier { get; private set; }
    
    internal readonly byte Id;
    internal readonly TcpConnection Connection;
    
    private readonly List<IPacketListener> listeners = [];
    
    public event Action<object> ConsumePacket = _ => {}; 
    
    internal TcpChannel(string identifier, byte id, TcpConnection connection) {
        Identifier = identifier;
        Id = id;
        Connection = connection;
    }
    
    public void Send(object packet) {
        var packetId = Connection.PacketIdentifier.FetchPacketId(packet);
        if (!Connection.IsInitialized && packetId >= 0)
            throw new ArgumentException("Cannot send non-system packets while initializing");
        
        try {
            Connection.PacketIdentifier.Encode(Connection.NativeStream, packet, packetId, this);
        } catch {
            Connection.Disconnect(DisconnectReason.Exception);
        }
    }

    public void RegisterPacketListener<T>(Action<T> consumer) {
        lock (listeners) {
            listeners.Add(new PacketListener<T>(consumer));
        }
    }

    internal void AcceptPacket(object packet) {
        var type = packet.GetType();

        lock (listeners) {
            var listener = listeners.Where(l => l.Type == type);
            if (listener.Any()) {
                foreach (var packetListener in listener) {
                    packetListener.Consume(packet);
                }
                return;
            }
        }
        
        ConsumePacket(packet);
    }
}

internal interface IPacketListener {
    public Type Type { get; }
    public void Consume(object obj);
}

internal class PacketListener<T> : IPacketListener {
    public Type Type { get; }
    public readonly Action<T> Consumer;
    
    public PacketListener(Action<T> consumer) {
        Type = typeof(T);
        Consumer = consumer;
    }

    public void Consume(object obj) {
        if (obj.GetType() != Type)
            throw new ArgumentException($"Cannot consume packet with type other than registered type. Expected {Type}, got {obj.GetType()}");
        Consumer.Invoke((T) obj);
    }
}