namespace UltoLibraryNew.Nbt.Values;

public class NbtBool : NbtComponent {
    public readonly bool Value;
    
    public NbtBool(bool value) {
        Value = value;
    }

    public override NbtType Type => NbtType.Bool;
    
    public override bool AsBool() {
        return Value;
    }

    public override byte AsByte() {
        return (byte) (Value ? 1 : 0);
    }

    public override byte[] AsByteArray() {
        throw new InvalidCastException("Cannot cast nbt type bool to byte array");
    }

    public override short AsShort() {
        return (short) (Value ? 1 : 0);
    }

    public override int AsInt() {
        return Value ? 1 : 0;
    }

    public override long AsLong() {
        return Value ? 1 : 0;
    }

    public override string AsString() {
        return Value.ToString();
    }

    public override double AsDouble() {
        return Value ? 1 : 0;
    }

    public override DateTime AsDateTime() {
        throw new InvalidCastException("Cannot cast nbt type bool to DateTime");
    }

    public override TimeSpan AsTimeSpan() {
        throw new InvalidCastException("Cannot cast nbt type bool to TimeSpan");
    }

    public override NbtContainer AsContainer() {
        throw new InvalidCastException("Cannot cast nbt type bool to NbtContainer");
    }

    public override NbtArray AsArray() {
        throw new InvalidCastException("Cannot cast nbt type bool to NbtArray");
    }

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write((byte) (Value ? 1 : 0));
    }
}