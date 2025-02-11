using System.Net;
using System.Net.Sockets;
using UltoLibraryNew.Network.ConnectionInitializers;

namespace UltoLibraryNew.Network;

public class TcpServer {
    public Task CloseTask => closeTaskSource?.Task ?? Task.CompletedTask;

    private readonly IPEndPoint bindEndPoint;
    private readonly List<TcpConnection> connections = [];
    private TaskCompletionSource? closeTaskSource;
    private TcpListener? listener;
    
    public event Action<ServerConnectionInitializer> OnConnectionInitialize = _ => { };
    public event Action<TcpConnection> OnConnect = _ => { };

    public TcpServer(string address, ushort port) : this(IPAddress.Parse(address), port) { }
    public TcpServer(IPAddress address, ushort port) : this(new IPEndPoint(address, port)) { }
    public TcpServer(IPEndPoint bindEndPoint) {
        this.bindEndPoint = bindEndPoint;
    }

    public void Bind() {
        closeTaskSource = new TaskCompletionSource();
        listener = new TcpListener(bindEndPoint);
        listener.Start();
        listener.BeginAcceptTcpClient(Callback, null);
        return;
        
        void Callback(IAsyncResult result) {
            var client = listener.EndAcceptTcpClient(result);
            ConsumeClient();

            listener?.BeginAcceptTcpClient(Callback, null);
            return;

            void ConsumeClient() {
                try {
                    var endPoint = (IPEndPoint) client.Client.RemoteEndPoint!;
                    var ip = endPoint.Address.ToString();
                    var port = (ushort) endPoint.Port;

                    NetworkStream stream;
                    try {
                        stream = client.GetStream();
                    } catch {
                        return;
                    }

                    var connection = new TcpConnection(ip, port, stream, true);
                    var initializer = new ServerConnectionInitImpl(connection, this);
                    OnConnectionInitialize(initializer);
                    connection.Initialize(initializer);
                    connections.Add(connection);
                    OnConnect(connection);
                } catch { }
            }
        }
    }

    public void Disconnect(TcpConnection connection) {
        Disconnect(connection, DisconnectReason.Disconnect);
    }

    internal void Disconnect(TcpConnection connection, DisconnectReason reason) {
        connections.Remove(connection);
        connection.OnDisconnecting(reason);
    }

    public void Close() {
        listener?.Dispose();
        listener = null;
        closeTaskSource?.TrySetResult();
        closeTaskSource = null;
        connections.ForEach(c => Disconnect(c, DisconnectReason.Closing));
    }
}