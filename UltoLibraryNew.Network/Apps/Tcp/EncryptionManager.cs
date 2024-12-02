namespace UltoLibraryNew.Network.Apps.Tcp;

public interface EncryptionManager {
    public void InitTcpStream(Stream tcpStream);
    
    public TcpEncryptionStream EncryptionStream { get; }
    public TcpDecryptionStream DecryptionStream { get; }
}