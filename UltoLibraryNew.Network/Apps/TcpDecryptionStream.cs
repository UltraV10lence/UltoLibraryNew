namespace UltoLibraryNew.Network.Apps;

public abstract class TcpDecryptionStream : Stream {
    protected readonly Stream TcpStream;
        
    protected TcpDecryptionStream(Stream tcpStream) {
        TcpStream = tcpStream;
    }

    public override void Flush() {
    }
    
    public override void Write(byte[] buffer, int offset, int count) {
        throw new InvalidOperationException("Cannot write to this stream");
    }

    public override long Seek(long offset, SeekOrigin origin) {
        return Position = offset;
    }

    public override void SetLength(long value) {
        Position = value;
    }

    public override bool CanWrite => false;
    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override long Length => TcpStream.Length;

    public override long Position {
        get => TcpStream.Position;
        set => throw new InvalidOperationException("Cannot seek this stream");
    }
}