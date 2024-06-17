using System.Net.Sockets;
using Timer = System.Timers.Timer;

namespace UltoLibraryNew.Network.Apps.Tcp;

public class TcpNetConnection {
    public Task CloseTask => CloseSource.Task;
    public event Action OnDisconnect = () => { };
    public event Action OnPacketTick;
    public event Action<ByteBuf> OnPacket = _ => { };
    public readonly TcpClient TcpClient;
    public string RemoteIp { get; internal set; } = null!;
    public ushort RemotePort { get; internal set; }
    
    protected readonly TaskCompletionSource CloseSource = new();
    private readonly PacketReader reader;
    private Timer? timeout, ping;

    public TcpNetConnection(TcpClient client) {
        OnPacketTick = ResetTimeout;
        reader = new PacketReader(this);
        TcpClient = client;
        ping = new Timer(5000);
        ping.Elapsed += (_, _) => {
            Send(new ByteBuf(), true);
        };
        ping.Start();
    }

    private void ResetTimeout() {
        timeout?.Dispose();
        timeout = new Timer(15000);
        timeout.Elapsed += (_, _) => Disconnecting();
        timeout.Start();
    }

    public void Send(ByteBuf data) => Send(data, false);
    internal void Send(ByteBuf data, bool isPing) {
        lock (TcpClient) {
            data.EnterReadOnlyMode();
            var len = BitConverter.GetBytes(data.Length);
            if (isPing) len[7] |= 0b10000000;
            TcpClient.GetStream().Write(len);
            data.CopyTo(TcpClient.GetStream());
            data.Stream.Dispose();
        }
    }

    internal async void ReceivePackets() {
        await Task.Factory.StartNew(async () => {
            var buf = new byte[512];
            int len;
            while (!CloseTask.IsCompleted) {
                try {
                    if ((len = await TcpClient.GetStream().ReadAsync(buf)) == 0) {
                        await Task.Delay(10);
                        continue;
                    }

                    var read = buf[..len];
                    reader.AddData(read);
                }
                catch (SocketException) {
                    Disconnecting();
                }
            }
        });
    }

    internal void Disconnecting() {
        timeout?.Dispose();
        ping?.Dispose();
        CloseSource.SetResult();
        OnDisconnect();
    }

    internal void Packet(ByteBuf data) {
        OnPacket(data);
    }
    
    internal void PacketTick() {
        OnPacketTick();
    }
}