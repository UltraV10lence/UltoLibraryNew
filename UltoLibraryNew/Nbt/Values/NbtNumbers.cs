namespace UltoLibraryNew.Nbt.Values;

public class NbtByte : NbtNumber<byte> {
    public override NbtType Type => NbtType.Byte;

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write((byte) Value);
    }

    public NbtByte(byte value) : base(value) { }
}

public class NbtShort : NbtNumber<short> {
    public override NbtType Type => NbtType.Short;

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write((short) Value);
    }

    public NbtShort(short value) : base(value) { }
}

public class NbtInteger : NbtNumber<int> {
    public override NbtType Type => NbtType.Integer;

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write7BitEncodedInt((int) Value);
    }

    public NbtInteger(int value) : base(value) { }
}

public class NbtLong : NbtNumber<long> {
    public override NbtType Type => NbtType.Long;

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write7BitEncodedInt64((long) Value);
    }

    public override DateTime AsDateTime() {
        return DateTime.FromBinary((long) Value);
    }

    public NbtLong(long value) : base(value) { }
}

public class NbtDouble : NbtNumber<double> {
    public override NbtType Type => NbtType.Double;

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write((double) Value);
    }

    public NbtDouble(double value) : base(value) { }
}