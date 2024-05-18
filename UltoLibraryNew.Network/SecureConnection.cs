using UltoLibraryNew.Network.Apps;
using UltoLibraryNew.Network.Apps.Packets;
using UltoLibraryNew.Network.Apps.Tcp;

namespace UltoLibraryNew.Network;

public class SecureConnection(NetConnection conn, bool isServer) {
    public readonly NetConnection Connection = conn;
    private byte[] key = null!;

    internal void Init() {
        Connection.RegisterPacketListener(Connection.LoginChannel, buf => {
            try {
                if (isServer) {
                    var encryptedKey = UltoBytes.EncryptRsa(key, buf.ReadBytes(buf.ReadUShort()));
                    Connection.LoginChannel.Send(new ByteBuf().WriteUShort((ushort)encryptedKey.Length).WriteBytes(encryptedKey));
                    Connection.TriggerPacketsSend();
                    Connection.LoginChannel.Close();
                    
                    var serverConn = (TcpNetServer.ServerConnection)Connection;
                    serverConn.PingTask.Start();
                    serverConn.ReceivePing.Start();
                    serverConn.LoginTimeout.Dispose();
                    
                    Connection.IsAuthorized = true;
                    Connection.Connected();
                } else {
                    key = UltoBytes.DecryptRsa(buf.ReadBytes(buf.ReadUShort()), key);
                    Connection.LoginChannel.Close();
                    
                    var clientConn = (TcpNetClient)Connection;
                    clientConn.PingTask.Start();
                    clientConn.ReceivePing.Start();
                    clientConn.LoginTimeout.Dispose();
                    
                    Connection.IsAuthorized = true;
                    Connection.Connected();
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                Connection.Disconnect(DisconnectReason.Exception);
            }
            return PacketAction.Stop;
        });
        if (isServer) InitServer();
        else InitClient(Connection.LoginChannel);
    }

    private void InitServer() {
        key = UltoBytes.GenerateAesKey();
    }

    private void InitClient(NetChannel ch) {
        var keys = UltoBytes.GenerateRsaKeys(false);
        key = keys.privateKey;
        ch.Send(new ByteBuf().WriteUShort((ushort)keys.publicKey.Length).WriteBytes(keys.publicKey));
    }

    public void Encrypt(ByteBuf data) {
        if (!Connection.IsAuthorized) return;
        var encrypted = new MemoryStream((int)data.Stream.Position);
        data.Stream.Seek(0, SeekOrigin.Begin);
        
        UltoBytes.EncryptAesStream(data.Stream, encrypted, key);
        data.Stream.Dispose();
        data.Stream = encrypted;
    }

    public void Decrypt(ByteBuf data) {
        if (!Connection.IsAuthorized) return;
        var decrypted = new MemoryStream((int)data.Stream.Position);
        data.Stream.Seek(0, SeekOrigin.Begin);
        
        UltoBytes.DecryptAesStream(data.Stream, decrypted, key);
        data.Stream.Dispose();
        decrypted.Seek(0, SeekOrigin.Begin);
        data.Stream = decrypted;
    }
}