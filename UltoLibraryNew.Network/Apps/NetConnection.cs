using UltoLibraryNew.Network.Apps.Packets;
using UltoLibraryNew.Network.Apps.Tcp;

namespace UltoLibraryNew.Network.Apps; 

public abstract class NetConnection {
    public event Action OnConnect = null!;
    public event Action<DisconnectReason> OnDisconnect = null!;
    public event Action<Exception> OnException = null!;
    
    public readonly bool IsServer;
    public bool IsClient => !IsServer;
    public bool IsAuthorized { get; internal set; }

    public readonly string RemoteIp;
    public readonly int RemotePort;

    public UnknownChannelPolitics GotDataToUnknownChannel = UnknownChannelPolitics.Skip;
    
    internal TaskCompletionSource CloseSource = new ();
    public Task CloseTask => CloseSource.Task;

    public PacketReceiver PacketReceiver;

    protected NetChannel PingChannel => OpenChannel("csPing");
    protected NetChannel LoginChannel => OpenChannel("csLogin");
    
    protected NetConnection(string remoteIp, int remotePort, bool isServer) {
        OnException += _ => { };
        OnDisconnect += _ => { };
        OnConnect += () => { };
        RemoteIp = remoteIp;
        RemotePort = remotePort;
        IsServer = isServer;
        PacketReceiver = new PacketReceiver(this);
    }
    
    internal void Connected() {
        try {
            OnConnect();
        } catch (Exception e) {
            Exception(e);
        }
    }
    
    internal void Disconnecting(DisconnectReason reason) {
        try {
            OnDisconnect(reason);
        } catch (Exception e) {
            Exception(e);
        }
    }
    
    internal void Exception(Exception e) {
        try {
            OnException(e);
        } catch (Exception e1) {
            Console.WriteLine(e1);
        }
    }

    public abstract NetChannel OpenChannel(string name);
    public abstract bool HasChannel(string name);

    public void CloseChannel(string name) {
        CloseChannel(OpenChannel(name));
    }
    public abstract void CloseChannel(NetChannel channel);

    public abstract int ChannelsCount();

    public void RegisterPacketListener(string channelName, Func<NetChannel, ByteBuf, PacketAction> listener) {
        RegisterPacketListener(OpenChannel(channelName), listener);
    }
    public abstract void RegisterPacketListener(NetChannel channel, Func<NetChannel, ByteBuf, PacketAction> listener);
    
    public abstract void Disconnect(DisconnectReason reason);
}