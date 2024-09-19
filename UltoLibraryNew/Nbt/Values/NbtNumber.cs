using System.Numerics;

namespace UltoLibraryNew.Nbt.Values;

public abstract class NbtNumber<T> : NbtComponent where T : IBinaryNumber<T> {
    protected readonly dynamic Value;

    protected NbtNumber(T value) {
        Value = value;
    }
    
    public abstract override NbtType Type { get; }

    public override bool AsBool() {
        throw new InvalidCastException("Cannot cast nbt number to bool");
    }

    public override byte AsByte() {
        return (byte) Value;
    }

    public override byte[] AsByteArray() {
        throw new InvalidCastException("Cannot cast nbt number to byte array");
    }

    public override short AsShort() {
        return (short) Value;
    }

    public override int AsInt() {
        return (int) Value;
    }

    public override long AsLong() {
        return (long) Value;
    }

    public override string AsString() {
        return Value.ToString()!;
    }

    public override double AsDouble() {
        return (double) Value;
    }

    public override DateTime AsDateTime() {
        throw new InvalidCastException("Cannot cast nbt number to DateTime");
    }

    public override TimeSpan AsTimeSpan() {
        throw new InvalidCastException("Cannot cast nbt number to TimeSpan");
    }

    public override NbtContainer AsContainer() {
        throw new InvalidCastException("Cannot cast nbt number to NbtContainer");
    }

    public override NbtArray AsArray() {
        throw new InvalidCastException("Cannot cast nbt number to NbtArray");
    }

    protected abstract override void WriteValue(BinaryWriter writer);
}