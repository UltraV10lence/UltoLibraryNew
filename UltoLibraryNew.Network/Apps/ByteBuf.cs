using System.Text;

namespace UltoLibraryNew.Network.Apps;

public class ByteBuf {
    public readonly Stream Stream;
    private bool readOnly;
    private long length;
    public long Length => readOnly ? length : Stream.Position;

    public void EnterReadOnlyMode() {
        readOnly = true;
        length = Stream.Position;
        Stream.Seek(0, SeekOrigin.Begin);
    }

    public ByteBuf() {
        Stream = new MemoryStream();
    }
    public ByteBuf(byte[] data) {
        Stream = new MemoryStream(data);
    }
    internal ByteBuf(Stream data) {
        Stream = data;
    }

    public ByteBuf Write(bool data) {
        CheckCanWrite();
        Stream.WriteByte((byte) (data ? 1 : 0));
        return this;
    }
    public ByteBuf Write(byte data) {
        CheckCanWrite();
        Stream.WriteByte(data);
        return this;
    }
    public ByteBuf Write(byte[] data) {
        CheckCanWrite();
        Stream.Write(data);
        return this;
    }
    public ByteBuf CopyFrom(Stream data) {
        CheckCanWrite();
        data.CopyTo(Stream);
        return this;
    }
    public ByteBuf Write(short data) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(data));
        return this;
    }
    public ByteBuf Write(ushort data) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(data));
        return this;
    }
    public ByteBuf Write(int data) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(data));
        return this;
    }
    public ByteBuf Write(uint data) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(data));
        return this;
    }
    public ByteBuf Write(long data) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(data));
        return this;
    }
    public ByteBuf Write(ulong data) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(data));
        return this;
    }
    public ByteBuf Write(double data) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(data));
        return this;
    }
    public ByteBuf Write(float data) {
        CheckCanWrite();
        Stream.Write(BitConverter.GetBytes(data));
        return this;
    }
    public ByteBuf Write(string data, Encoding? enc = null) {
        CheckCanWrite();
        enc ??= Encoding.UTF8;
        var str = enc.GetBytes(data);
        Write7BitEncodedInt(str.Length);
        Stream.Write(str);
        return this;
    }

    public ByteBuf Write7BitEncodedInt(int data) {
        CheckCanWrite();
        
        var uValue = (uint)data;
        while (uValue > 0x7Fu) {
            Stream.WriteByte((byte)(uValue | ~0x7Fu));
            uValue >>= 7;
        }

        Stream.WriteByte((byte)uValue);
        return this;
    }

    public ByteBuf Write7BitEncodedInt64(long value) {
        CheckCanWrite();
        
        var uValue = (ulong)value;
        while (uValue > 0x7Fu) {
            Stream.WriteByte((byte)((uint)uValue | ~0x7Fu));
            uValue >>= 7;
        }

        Stream.WriteByte((byte)uValue);
        return this;
    }

    public bool ReadBool() {
        return Stream.ReadByte() == 1;
    }

    public byte ReadByte() {
        return (byte) Stream.ReadByte();
    }
    public byte[] ReadBytes(int len) {
        var buf = new byte[len];
        Stream.ReadExactly(buf);
        return buf;
    }
    public void CopyTo(Stream data) {
        Stream.CopyTo(data);
    }
    public short ReadShort() {
        return BitConverter.ToInt16(ReadBytes(2));
    }
    public ushort ReadUShort() {
        return BitConverter.ToUInt16(ReadBytes(2));
    }
    public int ReadInt() {
        return BitConverter.ToInt32(ReadBytes(4));
    }
    public uint ReadUInt() {
        return BitConverter.ToUInt32(ReadBytes(4));
    }
    public long ReadLong() {
        return BitConverter.ToInt64(ReadBytes(8));
    }
    public ulong ReadULong() {
        return BitConverter.ToUInt64(ReadBytes(8));
    }
    public double ReadDouble() {
        return BitConverter.ToDouble(ReadBytes(8));
    }
    public float ReadFloat() {
        return BitConverter.ToSingle(ReadBytes(4));
    }
    public string ReadString(Encoding? enc = null) {
        enc ??= Encoding.UTF8;
        var len = Read7BitEncodedInt();
        return enc.GetString(ReadBytes(len));
    }
    
    public int Read7BitEncodedInt() {
        uint result = 0;
        byte byteReadJustNow;

        const int maxBytesWithoutOverflow = 4;
        for (var shift = 0; shift < maxBytesWithoutOverflow * 7; shift += 7) {
            byteReadJustNow = (byte) Stream.ReadByte();
            result |= (byteReadJustNow & 0x7Fu) << shift;

            if (byteReadJustNow <= 0x7Fu) {
                return (int)result;
            }
        }

        byteReadJustNow = (byte) Stream.ReadByte();
        if (byteReadJustNow > 0b_1111u) {
            throw new IOException("Cannot read 7-bit encoded integer");
        }

        result |= (uint)byteReadJustNow << (maxBytesWithoutOverflow * 7);
        return (int)result;
    }
    
    public long Read7BitEncodedInt64() {
        ulong result = 0;
        byte byteReadJustNow;

        const int maxBytesWithoutOverflow = 9;
        for (var shift = 0; shift < maxBytesWithoutOverflow * 7; shift += 7) {
            byteReadJustNow = (byte) Stream.ReadByte();
            result |= (byteReadJustNow & 0x7Ful) << shift;

            if (byteReadJustNow <= 0x7Fu) {
                return (long)result;
            }
        }

        byteReadJustNow = (byte) Stream.ReadByte();
        if (byteReadJustNow > 0b_1u) {
            throw new IOException("Cannot read 7-bit encoded long");
        }

        result |= (ulong)byteReadJustNow << (maxBytesWithoutOverflow * 7);
        return (long)result;
    }

    public void Move(long index, SeekOrigin origin = SeekOrigin.Begin) {
        Stream.Seek(index, origin);
    }

    public void CheckCanWrite() {
        if (readOnly) throw new Exception("Cannot write into stream when readonly mode is enabled");
    }
}