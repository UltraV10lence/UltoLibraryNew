using System.Text;

namespace UltoLibraryNew.Network.Apps;

public sealed class ByteBuf {
    private bool isInReadMode;
    internal MemoryStream Stream;

    public ByteBuf() {
        Stream = new MemoryStream();
        isInReadMode = false;
    }

    public ByteBuf(byte[] data) {
        Stream = new MemoryStream(data);
        EnterReadMode();
    }

    ~ByteBuf() {
        Stream.Dispose();
    }

    public ByteBuf WriteInt(int i) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(i));
        return this;
    }
    public ByteBuf WriteUInt(uint ui) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(ui));
        return this;
    }

    public int ReadInt() {
        return BitConverter.ToInt32(Read(4));
    }
    public uint ReadUInt() {
        return BitConverter.ToUInt32(Read(4));
    }

    public ByteBuf WriteShort(short s) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(s));
        return this;
    }
    public ByteBuf WriteUShort(ushort us) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(us));
        return this;
    }

    public short ReadShort() {
        return BitConverter.ToInt16(Read(2));
    }
    public ushort ReadUShort() {
        return BitConverter.ToUInt16(Read(2));
    }

    public ByteBuf WriteLong(long l) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(l));
        return this;
    }
    public ByteBuf WriteULong(ulong ul) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(ul));
        return this;
    }

    public long ReadLong() {
        return BitConverter.ToInt64(Read(8));
    }
    public ulong ReadUlong() {
        return BitConverter.ToUInt64(Read(8));
    }

    public ByteBuf WriteByte(byte b) {
        CheckCanWrite();
        Stream.WriteByte(b);
        return this;
    }
    
    public byte ReadByte() {
        return Read(1)[0];
    }

    public ByteBuf WriteBytes(byte[] b) {
        CheckCanWrite();
        Stream.Write(b);
        return this;
    }
    
    public byte[] ReadBytes(int length) {
        return Read(length);
    }

    public ByteBuf WriteString(string s) {
        CheckCanWrite();
        var data = Encoding.UTF8.GetBytes(s);
        Stream.Write(BitConverter.GetBytes((ushort) data.Length));
        Stream.Write(data);
        return this;
    }
    
    public string ReadString() {
        var length = ReadUShort();
        return Encoding.UTF8.GetString(Read(length));
    }

    public ByteBuf WriteDouble(double d) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(d));
        return this;
    }

    public double ReadDouble() {
        return BitConverter.ToDouble(Read(sizeof(double)));
    }

    public ByteBuf Move(int bytes) {
        Stream.Seek(bytes, SeekOrigin.Current);
        return this;
    }
    
    private void CheckCanWrite() {
        if (isInReadMode || !Stream.CanWrite) throw new IOException("Cannot write to ByteBuf in read mode.");
    }
    
    private void CheckCanRead() {
        if (!isInReadMode || !Stream.CanRead) throw new IOException("Cannot read to ByteBuf in write mode.");
    }

    private byte[] Read(int bytesToRead) {
        CheckCanRead();
        var buf = new byte[bytesToRead];
        Stream.ReadExactly(buf);
        return buf;
    }

    public void EnterReadMode() {
        isInReadMode = true;
        Stream.Seek(0, SeekOrigin.Begin);
    }
}