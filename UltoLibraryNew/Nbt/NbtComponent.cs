using System.IO.Compression;
using System.Text;
using UltoLibraryNew.Nbt.Values;

namespace UltoLibraryNew.Nbt;

public abstract class NbtComponent {
    public abstract NbtType Type { get; }

    public abstract bool AsBool();
    public abstract byte AsByte();
    public abstract byte[] AsByteArray();
    public abstract short AsShort();
    public abstract int AsInt();
    public abstract long AsLong();
    public abstract string AsString();
    public abstract double AsDouble();
    public abstract DateTime AsDateTime();
    public abstract TimeSpan AsTimeSpan();
    public abstract NbtContainer AsContainer();
    public abstract NbtArray AsArray();

    protected abstract void WriteValue(BinaryWriter writer);

    public void WriteCompressed(Stream writeTo, bool leaveOpen = false) {
        var compression = UltoBytes.CompressionStream(writeTo, leaveOpen);
        Write(compression);
    }

    public void Write(Stream data, bool withType = true, bool leaveOpen = false) {
        using (var writer = new BinaryWriter(data, Encoding.UTF8, leaveOpen)) {
            Write(writer, withType);
        }
    }
    
    public void Write(BinaryWriter writer, bool withType = true) {
        if (withType) writer.Write((byte) Type);
        WriteValue(writer);
    }

    public override string ToString() {
        return AsString();
    }

    public static NbtComponent ReadCompressed(Stream readFrom, bool leaveOpen = false) {
        var compression = UltoBytes.DecompressionStream(readFrom, leaveOpen);
        return Read(compression);
    }

    public static NbtComponent Read(Stream data, bool leaveOpen = false) {
        using (var reader = new BinaryReader(data, Encoding.UTF8, leaveOpen)) {
            return Read(reader);
        }
    }

    public static NbtComponent Read(BinaryReader reader) {
        var type = (NbtType) reader.ReadByte();
        switch (type) {
            case NbtType.Bool:
                return reader.ReadByte() == 0 ? new NbtBool(false) : new NbtBool(true);
            case NbtType.Byte:
                return new NbtByte(reader.ReadByte());
            case NbtType.ByteArray:
                return new NbtByteArray(reader.ReadBytes(reader.Read7BitEncodedInt()));
            case NbtType.Short:
                return new NbtShort(reader.ReadInt16());
            case NbtType.Integer:
                return new NbtInteger(reader.Read7BitEncodedInt());
            case NbtType.Long:
                return new NbtLong(reader.Read7BitEncodedInt64());
            case NbtType.String:
                return new NbtString(reader.ReadString());
            case NbtType.Double:
                return new NbtDouble(reader.ReadDouble());
            case NbtType.DateTime:
                return new NbtDateTime(DateTime.FromBinary(reader.Read7BitEncodedInt64()));
            case NbtType.TimeSpan:
                return new NbtTimeSpan(TimeSpan.FromTicks(reader.Read7BitEncodedInt64()));
            case NbtType.Container:
                var length = reader.Read7BitEncodedInt();
                var container = new NbtContainer();
                
                for (var i = 0; i < length; i++) {
                    var name = reader.ReadString();
                    var value = Read(reader);
                    container.Add(name, value);
                }

                return container;
            case NbtType.Array:
                length = reader.Read7BitEncodedInt();
                var array = new NbtArray();
                for (var i = 0; i < length; i++) {
                    array.Add(Read(reader));
                }

                return array;
        }

        throw new ArgumentException($"Cannot read nbt type {type}");
    }
}