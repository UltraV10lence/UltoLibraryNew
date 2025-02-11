using System.Net;
using System.Net.Sockets;
using UltoLibraryNew.Network.ConnectionInitializers;

namespace UltoLibraryNew.Network;

public class TcpClient {
    public TcpConnection? Connection { get; private set; }
    public Task CloseTask => closeTaskSource?.Task ?? Task.CompletedTask;
    
    private readonly IPEndPoint remoteEndpoint;
    private System.Net.Sockets.TcpClient? client;
    private TaskCompletionSource? closeTaskSource;
    
    public event Action<ConnectionInitializer> OnConnectionInitialize = _ => { };
    public event Action<TcpConnection> OnConnect = _ => { };
    
    public TcpClient(string remoteIp, ushort remotePort) : this(IPAddress.Parse(remoteIp), remotePort) { }
    public TcpClient(IPAddress remoteIp, ushort remotePort) : this(new IPEndPoint(remoteIp, remotePort)) { }
    public TcpClient(IPEndPoint endPoint) {
        remoteEndpoint = endPoint;
    }

    public void Connect() {
        Disconnect(DisconnectReason.Reconnect);
        closeTaskSource = new TaskCompletionSource();

        try {
            client = new System.Net.Sockets.TcpClient();

            NetworkStream stream;
            try {
                client.Connect(remoteEndpoint);
                stream = client.GetStream();
            } catch {
                Disconnect(DisconnectReason.StreamClosed);
                return;
            }
        
            Connection = new TcpConnection(remoteEndpoint.Address.ToString(), (ushort) remoteEndpoint.Port, stream, false);
            var initializer = new ConnectionInitImpl(Connection, this);
            OnConnectionInitialize(initializer);
            Connection.Initialize(initializer);
            OnConnect(Connection);
        } catch {
            Disconnect(DisconnectReason.Exception);
        }
    }

    public void Disconnect() {
        Disconnect(DisconnectReason.Disconnect);
    }

    internal void Disconnect(DisconnectReason reason) {
        closeTaskSource?.TrySetResult();
        
        if (client == null) return;
        client.Close();
        client = null;
        
        if (Connection == null) return;
        Connection.OnDisconnecting(reason);
        Connection = null;
    }
}