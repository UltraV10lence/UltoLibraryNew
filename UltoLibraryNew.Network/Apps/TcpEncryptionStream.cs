namespace UltoLibraryNew.Network.Apps;

public abstract class TcpEncryptionStream : Stream {
    protected readonly Stream TcpStream;
        
    protected TcpEncryptionStream(Stream tcpStream) {
        TcpStream = tcpStream;
    }

    public override void Flush() {
        TcpStream.Flush();
    }
    
    public override int Read(byte[] buffer, int offset, int count) {
        throw new InvalidOperationException("Cannot read from this stream");
    }

    public override long Seek(long offset, SeekOrigin origin) {
        return Position = offset;
    }

    public override void SetLength(long value) {
        Position = value;
    }

    public override bool CanWrite => true;
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override long Length => 0;

    public override long Position {
        get => 0;
        set => throw new InvalidOperationException("Cannot seek this stream");
    }
}