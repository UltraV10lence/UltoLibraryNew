﻿using System.Diagnostics.CodeAnalysis;

namespace UltoLibraryNew; 

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class TriState {
    private readonly bool? value;
    private TriState(bool? value) {
        this.value = value;
    }

    public static readonly TriState TRUE = new (true);
    public static readonly TriState FALSE = new (false);
    public static readonly TriState USE_DEFAULT = new (null);
    
    public byte ToByte() {
        return (byte) (Equals(FALSE) ? 0 : Equals(TRUE) ? 1 : 2);
    }
    
    public bool ToBool() {
        return Equals(TRUE);
    }
    
    public static TriState FromByte(byte b) => b switch {
        0 => FALSE,
        1 => TRUE,
        2 => USE_DEFAULT,
        _ => throw new ArgumentOutOfRangeException(nameof(b), b, null)
    };
    
    public static bool operator ==(TriState left, TriState right) {
        return left.Equals(right);
    }
    public static bool operator !=(TriState left, TriState right) {
        return !(left == right);
    }
    public static bool operator ==(TriState left, bool right) {
        return left.Equals(right ? TRUE : FALSE);
    }
    public static bool operator !=(TriState left, bool right) {
        return !(left == right);
    }

    public override int GetHashCode() {
        return ToByte().GetHashCode();
    }

    public override bool Equals(object? obj) {
        return obj is TriState other && Equals(other);
    }

    public bool Equals(TriState other) {
        return value == other.value;
    }
    public bool Equals(bool other) {
        return Equals(other ? TRUE : FALSE);
    }
}