using System.Net;
using System.Net.Sockets;

namespace UltoLibraryNew.Network.Apps.Tcp;

public class TcpNetClient : TcpNetConnection {
    public event Action OnConnect = () => { };
    private readonly IPEndPoint localEndPoint;
    
    public TcpNetClient(IPEndPoint endPoint, EncryptionManager? encryptionManager = null) : base(new TcpClient(), encryptionManager) {
        localEndPoint = endPoint;
        RemoteIp = endPoint.Address.ToString();
        RemotePort = (ushort) endPoint.Port;
    }
    
    public TcpNetClient(IPAddress ip, ushort port, EncryptionManager? encryptionManager = null) : base(new TcpClient(), encryptionManager) {
        localEndPoint = new IPEndPoint(ip, port);
        RemoteIp = localEndPoint.Address.ToString();
        RemotePort = (ushort) localEndPoint.Port;
    }

    public TcpNetClient(string ip, ushort port, EncryptionManager? encryptionManager = null) : base(new TcpClient(), encryptionManager) {
        localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        RemoteIp = localEndPoint.Address.ToString();
        RemotePort = (ushort) localEndPoint.Port;
    }

    public void Connect() {
        TcpClient.Connect(localEndPoint);
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