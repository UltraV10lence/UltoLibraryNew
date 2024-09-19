namespace UltoLibraryNew.Nbt.Values;

public class NbtByteArray : NbtComponent {
    public readonly byte[] Value;
    
    public NbtByteArray(byte[] value) {
        Value = value;
    }
    
    public override NbtType Type => NbtType.ByteArray;
    
    public override bool AsBool() {
        throw new InvalidCastException("Cannot cast nbt byte array to bool");
    }

    public override byte AsByte() {
        throw new InvalidCastException("Cannot cast nbt byte array to byte");
    }

    public override byte[] AsByteArray() {
        return Value;
    }

    public override short AsShort() {
        throw new InvalidCastException("Cannot cast nbt byte array to short");
    }

    public override int AsInt() {
        throw new InvalidCastException("Cannot cast nbt byte array to integer");
    }

    public override long AsLong() {
        throw new InvalidCastException("Cannot cast nbt byte array to long");
    }

    public override string AsString() {
        return $"[{string.Join(", ", Value)}]";
    }

    public override double AsDouble() {
        throw new InvalidCastException("Cannot cast nbt byte array to double");
    }

    public override DateTime AsDateTime() {
        throw new InvalidCastException("Cannot cast nbt byte array to DateTime");
    }

    public override TimeSpan AsTimeSpan() {
        throw new InvalidCastException("Cannot cast nbt byte array to TimeSpan");
    }

    public override NbtContainer AsContainer() {
        throw new InvalidCastException("Cannot cast nbt byte array to NbtContainer");
    }

    public override NbtArray AsArray() {
        throw new InvalidCastException("Cannot cast nbt byte array to NbtArray");
    }

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write7BitEncodedInt(Value.Length);
        writer.Write(Value);
    }
}