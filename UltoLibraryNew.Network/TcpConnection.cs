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
    private readonly Dictionary<byte, TcpChannel> registeredChannels = [];
    private readonly TcpChannel systemChannel;
    private readonly Timer pingTimer = new(TimeSpan.FromSeconds(5));

    private Timer? timeoutTimer;
    private TcpClient? client;
    private TcpServer? server;

    public event Action<DisconnectReason> OnDisconnect = _ => { };
    
    internal TcpConnection(string remoteIp, ushort remotePort, NetworkStream nativeStream, bool isServerSide) {
        RemoteIp = remoteIp;
        RemotePort = remotePort;
        NativeStream = nativeStream;
        IsServerSide = isServerSide;
        systemChannel = new TcpChannel("sc_system", 0, this);
        
        registeredChannels.Add(0, systemChannel);
        systemChannel.RegisterPacketListener<ChannelsInfoPacket>(info => {
            if (IsServerSide) throw new ArgumentException("Cannot initialize channels from client");
            
            foreach (var channel in info.Channels) {
                registeredChannels.Add(channel.Key, new TcpChannel(channel.Value, channel.Key, this));
            }

            IsInitialized = true;
            client!.OnConnectDone(this);
        });

        pingTimer.Elapsed += (_, _) => {
            try {
                systemChannel.Send(new PingPacket());
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
        if (!IsServerSide) {
            client = ((ConnectionInitImpl) initializer).Client;
            return;
        }
        
        if (initializer is not ServerConnectionInitializer)
            throw new ArgumentException("Cannot initialize server connection without server connection initializer");

        byte i = 0;
        var init = (ServerConnectionInitImpl) initializer;
        server = init.Server;
        var channels = init.Channels.ToDictionary(_ => ++i);

        foreach (var channel in channels) {
            registeredChannels.Add(channel.Key, channel.Value);
        }
        
        SendChannelsRegistry();
        IsInitialized = true;
    }

    public TcpChannel GetChannel(string identifier) {
        return registeredChannels.Single(c => c.Value.Identifier == identifier).Value;
    }

    private void SendChannelsRegistry() {
        var dic = registeredChannels.ToDictionary(channel => channel.Key, channel => channel.Value.Identifier);
        systemChannel.Send(new ChannelsInfoPacket(dic));
    }

    private void StartReadPackets() {
        var buffer = new byte[256];

        Task.Run(async () => {
            while (!IsClosed) {
                int bytesRead;
                try {
                    bytesRead = await NativeStream.ReadAsync(buffer, 0, buffer.Length);
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
                    var channel = registeredChannels.Single(c => c.Key == meta.ChannelId);

                    try {
                        channel.Value.AcceptPacket(packet);
                    } catch {
                        Disconnect(DisconnectReason.Exception);
                    }
                });
            }
        });
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