using System.Net;
using System.Net.Sockets;

namespace UltoLibraryNew.Network.Apps.Tcp;

public class TcpNetServer {
    public Task CloseTask => closeSource.Task;
    public event Action<TcpNetConnection> OnConnect = _ => { };
    private readonly TaskCompletionSource closeSource = new();
    private readonly IPEndPoint localEndPoint;
    private readonly List<TcpNetConnection> connections = [];
    private TcpListener? server;
    
    public EncryptionManager? EncryptionManager { get; set; }

    public TcpNetServer(string ip, ushort port, EncryptionManager? encryptionManager = null) {
        EncryptionManager = encryptionManager;
        localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
    }
    public TcpNetServer(IPAddress ip, ushort port, EncryptionManager? encryptionManager = null) {
        EncryptionManager = encryptionManager;
        localEndPoint = new IPEndPoint(ip, port);
    }
    public TcpNetServer(IPEndPoint endPoint, EncryptionManager? encryptionManager = null) {
        EncryptionManager = encryptionManager;
        localEndPoint = endPoint;
    }

    public void Bind() {
        server = new TcpListener(localEndPoint);
        server.Start(16);

        Task.Factory.StartNew(async () => {
            try {
                while (!CloseTask.IsCompleted) {
                    var client = await server.AcceptTcpClientAsync();
                    var ipPort = (IPEndPoint) client.Client.RemoteEndPoint!;
                    var connection = new TcpNetConnection(client, EncryptionManager) {
                        RemoteIp = ipPort.Address.ToString(),
                        RemotePort = (ushort) ipPort.Port
                    };
                    connection.OnDisconnect += () => Disconnect0(connection);
                    lock (connections) {
                        connections.Add(connection);
                    }
                    OnConnect(connection);
                    connection.ReceivePackets();
                }
            }
            catch {
                Close();
            }
        }, TaskCreationOptions.LongRunning);
    }

    public void Disconnect(TcpNetConnection connection) {
        lock (connections) {
            connection.Disconnecting();
        }
    }

    private void Disconnect0(TcpNetConnection connection) {
        connections.Remove(connection);
        connection.TcpClient.Close();
    }

    public void Close() {
        lock (connections) {
            while (connections.Count > 0)
                connections[0].Disconnecting();
        }
        server?.Stop();
        server?.Dispose();
        closeSource.SetResult();
    }
}