using System.Net.Sockets;
using UltoLibraryNew.Network.ConnectionInitializers;
using UltoLibraryNew.Network.Packets;
using UltoLibraryNew.Network.Packets.SystemPackets;
using Timer = System.Timers.Timer;

namespace UltoLibraryNew.Network;

public class TcpConnection {
    public const int SystemChannelsCount = 1;
    public string RemoteIp { get; private set; }
    public ushort RemotePort { get; private set; }
    public bool IsInitialized { get; private set; }
    public bool IsClosed { get; private set; }
    public bool IsServerSide { get; }
    
    internal NetworkStream NativeStream { get; }
    internal readonly PacketTypeIdentifier PacketIdentifier = new();
    
    private readonly PacketReader reader = new();
    private readonly Timer pingTimer = new(TimeSpan.FromSeconds(5));
    private readonly List<IPacketListener> listeners = [];

    private Timer? timeoutTimer;
    private TcpClient? client;
    private TcpServer? server;
    
    public event Action<object> ConsumePacket = _ => {}; 
    public event Action<DisconnectReason> OnDisconnect = _ => { };
    
    internal TcpConnection(string remoteIp, ushort remotePort, NetworkStream nativeStream, bool isServerSide) {
        RemoteIp = remoteIp;
        RemotePort = remotePort;
        NativeStream = nativeStream;
        IsServerSide = isServerSide;

        pingTimer.Elapsed += (_, _) => {
            try {
                Send(new PingPacket());
            } catch { }
        };
        pingTimer.AutoReset = true;
        pingTimer.Start();
        
        ResetTimeoutTimer();
        StartReadPackets();
    }

    private void TimeoutExceeded() {
        Disconnect(DisconnectReason.Timeout);
    }

    public void ResetTimeoutTimer() {
        timeoutTimer?.Stop();

        if (timeoutTimer == null) {
            timeoutTimer = new Timer(TimeSpan.FromSeconds(15));
            timeoutTimer.Elapsed += (_, _) => {
                TimeoutExceeded();
                timeoutTimer.Stop();
            };
            timeoutTimer.AutoReset = false;
        }
        
        timeoutTimer.Start();
    }

    internal void Initialize(ConnectionInitializer initializer) {
        IsInitialized = true;
        
        if (!IsServerSide) {
            client = ((ConnectionInitImpl) initializer).Client;
            return;
        }
        
        if (initializer is not ServerConnectionInitializer)
            throw new ArgumentException("Cannot initialize server connection without server connection initializer");

        var init = (ServerConnectionInitImpl) initializer;
        server = init.Server;
    }

    private void StartReadPackets() {
        var buffer = new byte[256];

        Task.Run(async () => {
            while (!IsClosed) {
                int bytesRead;
                try {
                    bytesRead = await NativeStream.ReadAsync(buffer);
                } catch {
                    Disconnect(DisconnectReason.StreamClosed);
                    return;
                }

                if (bytesRead == 0) {
                    Task.Delay(10).Wait();
                    continue;
                }
        
                ResetTimeoutTimer();
                reader.AppendData(buffer, bytesRead, (buf, meta) => {
                    var packet = PacketIdentifier.Decode(meta.DataType, buf);

                    try {
                        AcceptPacket(packet);
                    } catch {
                        Disconnect(DisconnectReason.Exception);
                    }
                });
            }
        });
    }
    
    public void Send(object packet) {
        var packetId = PacketIdentifier.FetchPacketId(packet);
        if (!IsInitialized && packetId >= 0)
            throw new ArgumentException("Cannot send non-system packets while initializing");
        
        try {
            PacketIdentifier.Encode(NativeStream, packet, packetId, this);
        } catch {
            Disconnect(DisconnectReason.Exception);
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

    public void Disconnect() {
        Disconnect(DisconnectReason.Disconnect);
    }
    internal void Disconnect(DisconnectReason reason) {
        server?.Disconnect(this, reason);
        client?.Disconnect(reason);
        try {
            timeoutTimer?.Dispose();
            pingTimer.Dispose();
        } catch { }
        IsClosed = true;
    }

    internal void OnDisconnecting(DisconnectReason reason) {
        OnDisconnect(reason);
    }
}