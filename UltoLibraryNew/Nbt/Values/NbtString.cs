using System.Text;

namespace UltoLibraryNew.Nbt.Values;

public class NbtString : NbtComponent {
    public readonly string Value;

    public NbtString(string value) {
        Value = value;
    }
    
    public override NbtType Type => NbtType.String;
    
    public override bool AsBool() {
        throw new InvalidCastException("Cannot cast nbt string to bool");
    }

    public override byte AsByte() {
        throw new InvalidCastException("Cannot cast nbt string to byte");
    }

    public override byte[] AsByteArray() {
        return Encoding.UTF8.GetBytes(Value);
    }

    public override short AsShort() {
        throw new InvalidCastException("Cannot cast nbt string to short");
    }

    public override int AsInt() {
        throw new InvalidCastException("Cannot cast nbt string to integer");
    }

    public override long AsLong() {
        throw new InvalidCastException("Cannot cast nbt string to long");
    }

    public override string AsString() {
        return Value;
    }

    public override string AsFormattedString() {
        return $"\"{Value}\"";
    }

    public override double AsDouble() {
        throw new InvalidCastException("Cannot cast nbt string to double");
    }

    public override DateTime AsDateTime() {
        throw new InvalidCastException("Cannot cast nbt string to DateTime");
    }

    public override TimeSpan AsTimeSpan() {
        throw new InvalidCastException("Cannot cast nbt string to TimeSpan");
    }

    public override NbtContainer AsContainer() {
        throw new InvalidCastException("Cannot cast nbt string to NbtContainer");
    }

    public override NbtArray AsArray() {
        throw new InvalidCastException("Cannot cast nbt string to NbtArray");
    }

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write(Value);
    }
}