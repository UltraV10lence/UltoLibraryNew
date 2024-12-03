using System.Net;
using System.Net.Sockets;

namespace UltoLibraryNew.Network.Apps.Tcp;

public class TcpNetClient : TcpNetConnection {
    public event Action OnConnect = () => { };
    private readonly IPEndPoint remoteEndPoint;
    
    public TcpNetClient(IPEndPoint endPoint, EncryptionManager? encryptionManager = null) : base(new TcpClient(), encryptionManager) {
        remoteEndPoint = endPoint;
        RemoteIp = endPoint.Address.ToString();
        RemotePort = (ushort) endPoint.Port;
    }
    
    public TcpNetClient(IPAddress ip, ushort port, EncryptionManager? encryptionManager = null) : base(new TcpClient(), encryptionManager) {
        remoteEndPoint = new IPEndPoint(ip, port);
        RemoteIp = ip.ToString();
        RemotePort = port;
    }

    public TcpNetClient(string ip, ushort port, EncryptionManager? encryptionManager = null) : base(new TcpClient(), encryptionManager) {
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        RemoteIp = ip;
        RemotePort = port;
    }

    public void Connect() {
        TcpClient.Connect(remoteEndPoint);
        EncryptionManager?.InitTcpStream(TcpClient.GetStream());
        OnConnect();
        ReceivePackets();
    }

    public void Disconnect() {
        Close();
    }

    public void Close() {
        TcpClient.Close();
        TcpClient.Dispose();
        CloseSource.SetResult();
    }
}