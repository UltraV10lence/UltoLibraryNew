using System.Globalization;

namespace UltoLibraryNew.Nbt.Values;

public class NbtDateTime : NbtComponent {
    public readonly DateTime Value;

    public NbtDateTime(DateTime value) {
        Value = value;
    }
    
    public override NbtType Type => NbtType.DateTime;
    
    public override bool AsBool() {
        throw new InvalidCastException("Cannot cast nbt DateTime to bool");
    }

    public override byte AsByte() {
        throw new InvalidCastException("Cannot cast nbt DateTime to byte");
    }

    public override byte[] AsByteArray() {
        throw new InvalidCastException("Cannot cast nbt DateTime to byte array");
    }

    public override short AsShort() {
        throw new InvalidCastException("Cannot cast nbt DateTime to short");
    }

    public override int AsInt() {
        throw new InvalidCastException("Cannot cast nbt DateTime to integer");
    }

    public override long AsLong() {
        return Value.ToBinary();
    }

    public override string AsString() {
        return Value.ToString(CultureInfo.InvariantCulture);
    }

    public override double AsDouble() {
        throw new InvalidCastException("Cannot cast nbt DateTime to double");
    }

    public override DateTime AsDateTime() {
        return Value;
    }

    public override TimeSpan AsTimeSpan() {
        throw new InvalidCastException("Cannot cast nbt DateTime to TimeSpan");
    }

    public override NbtContainer AsContainer() {
        throw new InvalidCastException("Cannot cast nbt DateTime to NbtContainer");
    }

    public override NbtArray AsArray() {
        throw new InvalidCastException("Cannot cast nbt DateTime to NbtArray");
    }

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write7BitEncodedInt64(Value.ToBinary());
    }
}