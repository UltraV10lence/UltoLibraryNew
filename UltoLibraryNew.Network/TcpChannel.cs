namespace UltoLibraryNew.Network;

public class TcpChannel {
    public string Identifier { get; private set; }
    
    internal readonly byte Id;
    internal readonly TcpConnection Connection;
    
    internal TcpChannel(string identifier, byte id, TcpConnection connection) {
        Identifier = identifier;
        Id = id;
        Connection = connection;
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