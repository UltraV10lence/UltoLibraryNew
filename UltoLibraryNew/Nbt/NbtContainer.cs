using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace UltoLibraryNew.Nbt;

public class NbtContainer : NbtComponent, IDictionary<string, NbtComponent> {
    private readonly Dictionary<string, NbtComponent> map = [];
    
    public override NbtType Type => NbtType.Container;
    
    public override bool AsBool() {
        throw new InvalidCastException("Cannot cast NbtContainer to bool");
    }

    public override byte AsByte() {
        throw new InvalidCastException("Cannot cast NbtContainer to byte");
    }

    public override byte[] AsByteArray() {
        throw new InvalidCastException("Cannot cast NbtContainer to byte array");
    }

    public override short AsShort() {
        throw new InvalidCastException("Cannot cast NbtContainer to short");
    }

    public override int AsInt() {
        throw new InvalidCastException("Cannot cast NbtContainer to integer");
    }

    public override long AsLong() {
        throw new InvalidCastException("Cannot cast NbtContainer to long");
    }

    public override string AsString() {
        return '{' + string.Join(", ", map.Select(v => $"\"{v.Key}\": {v.Value.AsFormattedString()}")) + '}';
    }

    public override double AsDouble() {
        throw new InvalidCastException("Cannot cast NbtContainer to double");
    }

    public override DateTime AsDateTime() {
        throw new InvalidCastException("Cannot cast NbtContainer to DateTime");
    }

    public override TimeSpan AsTimeSpan() {
        throw new InvalidCastException("Cannot cast NbtContainer to TimeSpan");
    }

    public override NbtContainer AsContainer() {
        return this;
    }

    public override NbtArray AsArray() {
        throw new InvalidCastException("Cannot cast NbtContainer to NbtArray");
    }

    protected override void WriteValue(BinaryWriter writer) {
        writer.Write7BitEncodedInt(map.Count);
        foreach (var kv in map) {
            writer.Write(kv.Key);
            kv.Value.Write(writer);
        }
    }

    public IEnumerator<KeyValuePair<string, NbtComponent>> GetEnumerator() {
        return map.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    void ICollection<KeyValuePair<string, NbtComponent>>.Add(KeyValuePair<string, NbtComponent> keyValuePair) =>
        Add(keyValuePair.Key, keyValuePair.Value);

    public void Clear() {
        map.Clear();
    }

    public bool Contains(KeyValuePair<string, NbtComponent> item) {
        return map.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, NbtComponent>[] array, int arrayIndex) {
    }

    public bool Remove(KeyValuePair<string, NbtComponent> item) {
        return Remove(item.Key);
    }

    public int Count => map.Count;
    public bool IsReadOnly => false;
    public void Add(string key, NbtComponent value) {
        map.Add(key, value);
    }

    public bool ContainsKey(string key) {
        return map.ContainsKey(key);
    }

    public bool Remove(string key) {
        return map.Remove(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out NbtComponent value) {
        return map.TryGetValue(key, out value);
    }

    public NbtComponent this[string key] {
        get => map[key];
        set => map[key] = value;
    }

    public ICollection<string> Keys => map.Keys;
    public ICollection<NbtComponent> Values => map.Values;
}