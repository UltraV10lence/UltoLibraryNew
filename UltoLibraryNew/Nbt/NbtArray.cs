using System.Collections;
using UltoLibraryNew.Nbt.Values;

namespace UltoLibraryNew.Nbt;

public class NbtArray : NbtComponent, IList<NbtComponent> {
    private readonly List<NbtComponent> values = [];
    
    public override NbtType Type => NbtType.Array;
    
    public override bool AsBool() {
        throw new InvalidCastException("Cannot cast NbtArray to bool");
    }

    public override byte AsByte() {
        throw new InvalidCastException("Cannot cast NbtArray to byte");
    }

    public override byte[] AsByteArray() {
        throw new InvalidCastException("Cannot cast NbtArray to byte array");
    }

    public override short AsShort() {
        throw new InvalidCastException("Cannot cast NbtArray to short");
    }

    public override int AsInt() {
        throw new InvalidCastException("Cannot cast NbtArray to integer");
    }

    public override long AsLong() {
        throw new InvalidCastException("Cannot cast NbtArray to long");
    }

    public override string AsString() {
        return $"[{string.Join(", ", values.Select(v => v is NbtString s ? $"\"{s.Value}\"" : v.AsString()))}]";
    }

    public override double AsDouble() {
        throw new InvalidCastException("Cannot cast NbtArray to double");
    }

    public override DateTime AsDateTime() {
        throw new InvalidCastException("Cannot cast NbtArray to DateTime");
    }

    public override TimeSpan AsTimeSpan() {
        throw new InvalidCastException("Cannot cast NbtArray to TimeSpan");
    }

    public override NbtContainer AsContainer() {
        throw new InvalidCastException("Cannot cast NbtArray to NbtContainer");
    }

    public override NbtArray AsArray() {
        return this;
    }

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write7BitEncodedInt(values.Count);
        foreach (var value in values) {
            value.Write(writer);
        }
    }

    public IEnumerator<NbtComponent> GetEnumerator() {
        return values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Add(NbtComponent item) {
        values.Add(item);
    }

    public void Clear() {
        values.Clear();
    }

    public bool Contains(NbtComponent item) {
        return values.Contains(item);
    }

    public void CopyTo(NbtComponent[] array, int arrayIndex) {
        values.CopyTo(array, arrayIndex);
    }

    public bool Remove(NbtComponent item) {
        return values.Remove(item);
    }

    public int Count => values.Count;
    public bool IsReadOnly => false;
    public int IndexOf(NbtComponent item) {
        return values.IndexOf(item);
    }

    public void Insert(int index, NbtComponent item) {
        values.Insert(index, item);
    }

    public void RemoveAt(int index) {
        values.RemoveAt(index);
    }

    public NbtComponent this[int index] {
        get => values[index];
        set => values[index] = value;
    }
}