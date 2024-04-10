using System.Net;
using System.Net.Sockets;
using UltoLibraryNew.Network.Apps.Packets;
using Timer = System.Timers.Timer;

namespace UltoLibraryNew.Network.Apps.Tcp;

public class TcpNetServer(string ip, int port) : NetServer(ip, port) {
    private readonly List<ServerConnection> connections = [ ];

    public override void Bind() {
        CloseSource = new TaskCompletionSource();

        Task.Run(() => {
            try {
                var listener = new TcpListener(IPAddress.Parse(CurrentIp), CurrentPort);
                listener.Start(10);

                while (!CloseTask.IsCompleted) {
                    try {
                        var client = listener.AcceptTcpClientAsync().GetAwaiter().GetResult();
                        var ep = (IPEndPoint)client.Client.RemoteEndPoint!;

                        var connection = new ServerConnection(ep.Address.ToString(), ep.Port, this, client);
                        connections.Add(connection);
                        connection.Init();
                        connection.LoginTimeout.Start();

                        connection.OnConnect += () => {
                            Connect(connection);
                        };
                    } catch (Exception e) {
                        Exception(e);
                    }
                }
                
                listener.Stop();
            } catch (Exception e) {
                Exception(e);
                Close();
            }
        });
    }

    public override void Close() {
        while (connections.Count > 0) {
            Disconnect(connections[0], DisconnectReason.Disconnect);
        }
        
        CloseSource.SetResult();
    }

    public void Disconnect(NetConnection conn, DisconnectReason reason) {
        if (conn is not ServerConnection c) return;

        try {
            c.IsAuthorized = false;
            c.PingTask.Dispose();
            c.ReceivePing.Dispose();
            c.LoginTimeout.Dispose();
            c.RemoteRaw?.Close();
            c.RemoteRaw = null;
            c.Disconnecting(reason);
            c.CloseSource.SetResult();
            
            connections.Remove(c);
        } catch (Exception e) {
            Exception(e);
        }
    }
    
    internal class ServerConnection : NetConnection {
        public TcpClient? RemoteRaw { get; internal set; }
        private readonly Dictionary<string, NetChannel> channels = new();
        public readonly TcpNetServer Server;

        public readonly Timer ReceivePing = new(Limits.ToReceivePing);
        public readonly Timer PingTask = new(Limits.ToSendPing);
        public readonly Timer LoginTimeout = new(Limits.ToAuthorize);
        
        public ServerConnection(string remoteIp, int remotePort, TcpNetServer server, TcpClient remote) : base(remoteIp, remotePort, true) {
            Server = server;
            RemoteRaw = remote;
            PacketReceiver = new PacketReceiver(this);
        }

        public void Init() {
            RegisterPacketListener(PingChannel, (_, _) => {
                ReceivePing.Stop();
                ReceivePing.Start();
                return PacketAction.Skip;
            });
        
            RegisterPacketListener(LoginChannel, (c, buf) => {
                c.Close();
                PingTask.Start();
                ReceivePing.Start();
                LoginTimeout.Dispose();
                LoginChannel.Send(new ByteBuf());
                
                Connected();
                return PacketAction.Skip;
            });
            
            PingTask.Elapsed += (_, _) => {
                PingChannel.Send(new ByteBuf());
            };

            LoginTimeout.Elapsed += (_, _) => {
                Disconnect(DisconnectReason.AuthorizationTimeout);
            };

            ReceivePing.Elapsed += (_, _) => {
                Disconnect(DisconnectReason.Timeout);
            };
            
            PacketsTask();

            Task.Run(() => {
                while (!CloseTask.IsCompleted) {
                    SendTask.Wait();
                    TriggerPacketsSend();
                    SendSource = new TaskCompletionSource();
                }
            });
        }

        private async void PacketsTask() {
            await Task.Run(() => {
                var buf = new byte[1024];

                while (!CloseTask.IsCompleted) {
                    try {
                        stream ??= RemoteRaw!.GetStream();
                        var length = stream.ReadAsync(buf).AsTask().GetAwaiter().GetResult();
                        if (length == 0) continue;
                
                        var got = length == buf.Length ? buf : UltoBytes.SubArray(buf, 0, length);
                
                        try {
                            PacketReceiver.AddData(got);
                        } catch (Exception e) {
                            Exception(e);
                        }
                    } catch {
                        Disconnect(DisconnectReason.Disconnect);
                    }
                }
            });
        }

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
            } catch { }
        }
        
        private NetworkStream? stream;

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

        public override void Disconnect(DisconnectReason reason) {
            Server.Disconnect(this, reason);
        }
    }
}