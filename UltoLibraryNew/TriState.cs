using System.Diagnostics.CodeAnalysis;

namespace UltoLibraryNew; 

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class TriState {
    private readonly bool? value;
    private TriState(bool? value) {
        this.value = value;
    }

    public static readonly TriState TRUE = new(true);
    public static readonly TriState FALSE = new(false);
    public static readonly TriState USE_DEFAULT = new(null);
    
    public byte ToByte() => (byte) (Equals(FALSE) ? 0 : Equals(TRUE) ? 1 : 2);
    public bool ToBool() => Equals(TRUE);

    public static TriState FromByte(byte b) => b switch {
        0 => FALSE,
        1 => TRUE,
        2 => USE_DEFAULT,
        _ => throw new ArgumentOutOfRangeException(nameof(b), b, null)
    };
    
    public static bool operator==(TriState left, TriState right) => left.Equals(right);
    public static bool operator!=(TriState left, TriState right) => !(left == right);
    public static bool operator==(TriState left, bool right) => left.value.HasValue && left.value.Value == right;
    public static bool operator!=(TriState left, bool right) => !(left == right);

    public static explicit operator TriState(bool other) => new(other);
    public static explicit operator TriState(bool? other) => new(other);
    public static implicit operator bool?(TriState self) => self.value;
    public static implicit operator bool(TriState self) => self.ToBool();

    public override int GetHashCode() {
        return ToByte().GetHashCode();
    }

    public override bool Equals(object? obj) {
        if (obj is bool b) return value == b;
        return obj is TriState other && Equals(other);
    }

    public bool Equals(TriState other) {
        return value == other.value;
    }
}