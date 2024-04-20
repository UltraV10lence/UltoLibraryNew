using System.Net.Sockets;
using UltoLibraryNew.Network.Apps.Packets;
using Timer = System.Timers.Timer;

namespace UltoLibraryNew.Network.Apps.Tcp; 

public class TcpNetClient(string remoteIp, int remotePort) : NetConnection(remoteIp, remotePort, false) {
    public TcpClient? RemoteRaw { get; internal set; }
    private readonly Dictionary<string, NetChannel> channels = new();

    private readonly Timer receivePing = new(Limits.ToReceivePing);
    private readonly Timer pingTask = new(Limits.ToSendPing);
    private readonly Timer loginTimeout = new(Limits.ToAuthorize);
    
    public void Connect() {
        CloseSource = new TaskCompletionSource();
        
        RegisterPacketListener(PingChannel, (_, _) => {
            receivePing.Stop();
            receivePing.Start();
            return PacketAction.Skip;
        });
        
        RegisterPacketListener(LoginChannel, (c, _) => {
            c.Close();
            pingTask.Start();
            receivePing.Start();
            loginTimeout.Dispose();
            
            IsAuthorized = true;
            Connected();
            return PacketAction.Skip;
        });

        Task.Run(() => {
            try {
                RemoteRaw = new TcpClient();
                RemoteRaw.Connect(RemoteIp, RemotePort);

                pingTask.Elapsed += (_, _) => {
                    PingChannel.Send(new ByteBuf());
                };

                loginTimeout.Elapsed += (_, _) => {
                    Disconnect(DisconnectReason.AuthorizationTimeout);
                };

                receivePing.Elapsed += (_, _) => {
                    Disconnect(DisconnectReason.Timeout);
                };
                loginTimeout.Start();
                
                LoginChannel.Send(new ByteBuf());

                Task.Run(() => {
                    while (!CloseTask.IsCompleted) {
                        SendTask.Wait();
                        TriggerPacketsSend();
                        SendSource = new TaskCompletionSource();
                    }
                });
                
                try {
                    var buf = new byte[1024];
                    while (!CloseTask.IsCompleted) {
                        stream ??= RemoteRaw.GetStream();
                        var length = stream.ReadAsync(buf).AsTask().GetAwaiter().GetResult();
                        if (length == 0) continue;
                        
                        var got = length == buf.Length ? buf : UltoBytes.SubArray(buf, 0, length);
                        
                        try {
                            PacketReceiver.AddData(got);
                        } catch (Exception e) {
                            Exception(e);
                        }
                    }
                } catch {
                    Disconnect(DisconnectReason.Disconnect);
                }
            } catch (Exception e) {
                Exception(e);
                Disconnect(DisconnectReason.Exception);
            }
        });
    }

    public override NetChannel OpenChannel(string name) {
        if (HasChannel(name)) return channels[name];

        var c = new NetChannel(this, name);
        channels.Add(name, c);
        return c;
    }

    public override bool HasChannel(string name) {
        return channels.ContainsKey(name);
    }

    public override void CloseChannel(NetChannel channel) {
        channels.Remove(channel.ChannelName);
    }

    public override int ChannelsCount() {
        return channels.Count;
    }

    public override void RegisterPacketListener(NetChannel channel, Func<NetChannel, ByteBuf, PacketAction> listener) {
        channel.OnPacket.Add(listener);
    }

    private NetworkStream? stream;
    public override void TriggerPacketsSend() {
        if (RemoteRaw == null) return;
        try {
            stream ??= RemoteRaw.GetStream();
            var stop = false;

            while (!stop) {
                stop = true;
                
                foreach (var c in channels.Values) {
                    var data = c.GetNextNamedSlice();
                    if (data == null) continue;
                    stream.Write(data);
                    stop = false;
                }
            }
        } catch {
            // ignored
        }
    }

    public override void Disconnect(DisconnectReason reason) {
        try {
            IsAuthorized = false;
            pingTask.Dispose();
            loginTimeout.Dispose();
            receivePing.Dispose();
            RemoteRaw?.Close();
            RemoteRaw = null;
            Disconnecting(reason);
        } catch (Exception e) {
            Exception(e);
        }
        CloseSource.SetResult();
    }
}