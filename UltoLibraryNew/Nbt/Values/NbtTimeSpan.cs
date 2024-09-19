namespace UltoLibraryNew.Nbt.Values;

public class NbtTimeSpan : NbtComponent {
    public readonly TimeSpan Value;
    
    public NbtTimeSpan(TimeSpan value) {
        Value = value;
    }

    public override NbtType Type => NbtType.TimeSpan;
    
    public override bool AsBool() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to bool");
    }

    public override byte AsByte() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to byte");
    }

    public override byte[] AsByteArray() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to byte array");
    }

    public override short AsShort() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to short");
    }

    public override int AsInt() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to integer");
    }

    public override long AsLong() {
        return Value.Ticks;
    }

    public override string AsString() {
        return Value.ToString();
    }

    public override double AsDouble() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to double");
    }

    public override DateTime AsDateTime() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to DateTime");
    }

    public override TimeSpan AsTimeSpan() {
        return Value;
    }

    public override NbtContainer AsContainer() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to NbtContainer");
    }

    public override NbtArray AsArray() {
        throw new InvalidCastException("Cannot cast nbt TimeSpan to NbtArray");
    }

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write7BitEncodedInt64(Value.Ticks);
    }
}