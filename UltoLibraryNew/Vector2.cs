﻿using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace UltoLibraryNew; 

[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
public class Vector2 : IEquatable<Vector2> {
    public const double ComparisonEpsilon = 1E-6;
    
    public double X, Y;

    public double SquaredLength => X * X + Y * Y;
    public double Length => Math.Sqrt(SquaredLength);
    public Vector2 Normalized => new(X / Length, Y / Length);
    public Vector2 Abs => new(Math.Abs(X), Math.Abs(Y));
    
    public Vector2 ToBack => new(-X, -Y);
    public Vector2 ToRight => new(Y, -X);
    public Vector2 ToLeft => new(-Y, X);

    public Vector2(double x, double y) {
        X = x;
        Y = y;
    }

    public double this[int i] {
        get {
            return i switch {
                0 => X,
                1 => Y,
                _ => throw new IndexOutOfRangeException()
            };
        }
        set {
            switch (i) {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
            }
            throw new IndexOutOfRangeException();
        }
    }

    public void Normalize() {
        var norm = Normalized;
        X = norm.X;
        Y = norm.Y;
    }

    public Vector2 AbsoluteDistance2(Vector2 other) {
        return (other - this).Abs;
    }

    public double Distance(Vector2 other) {
        return (other - this).Length;
    }

    public double Angle(Vector2 other) {
        var a1 = Math.Atan2(Y, X);
        var a2 = Math.Atan2(other.Y, other.X);
        return (a2 - a1) * 180 / Math.PI;
    }

    public Vector2 Rotate(double angle) {
        var rad = angle * Math.PI / 180;
        var ca = Math.Cos(rad);
        var sa = Math.Sin(rad);
        
        return new Vector2(X * ca - Y * sa, X * sa + Y * ca);
    }

    public Vector2 Rotate(Vector2 center, double angle) {
        var translated = this - center;
        
        return translated.Rotate(angle) + center;
    }

    /// <summary>
    /// Returns the scalar product of two vectors
    /// </summary>
    /// <param name="other">The second vector</param>
    /// <returns>From -1 when the vectors face different directions, 0 when the vectors are perpendicular, to 1 when they face the same direction.</returns>
    public double Dot(Vector2 other) {
        var n1 = Normalized;
        var n2 = other.Normalized;
        return n1.X * n2.X + n1.Y * n2.Y;
    }
    
    public void MoveTowards(Vector2 target, double stepSize) {
        var trans = (target - this).Normalized * stepSize;
        X += trans.X;
        Y += trans.Y;
    }

    public override string ToString() {
        return $"({X}, {Y})";
    }

    public static Vector2 operator+(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator+(Vector2 a, double scalar) => new(a.X + scalar, a.Y + scalar);
    public static Vector2 operator-(Vector2 a) => new(-a.X, -a.Y);
    public static Vector2 operator-(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator-(Vector2 a, double scalar) => new(a.X - scalar, a.Y - scalar);
    public static Vector2 operator*(Vector2 a, Vector2 b) => new(a.X * b.X, a.Y * b.Y);
    public static Vector2 operator*(Vector2 a, double scalar) => new(a.X * scalar, a.Y * scalar);
    public static Vector2 operator/(Vector2 a, Vector2 b) => new(a.X / b.X, a.Y / b.Y);
    public static Vector2 operator/(Vector2 a, double scalar) => new(a.X / scalar, a.Y / scalar);
    public static bool operator==(Vector2 a, Vector2 b) => Math.Abs(a.X - b.X) < ComparisonEpsilon && Math.Abs(a.Y - b.Y) < ComparisonEpsilon;
    public static bool operator!=(Vector2 a, Vector2 b) => Math.Abs(a.X - b.X) >= ComparisonEpsilon || Math.Abs(a.Y - b.Y) >= ComparisonEpsilon;

    public static implicit operator Point(Vector2 self) => new((int) self.X, (int) self.Y);
    public static explicit operator Vector2(Point other) => new(other.X, other.Y);

    public static implicit operator System.Numerics.Vector2(Vector2 self) => new((int) self.X, (int) self.Y);
    public static explicit operator Vector2(System.Numerics.Vector2 other) => new(other.X, other.Y);

    public static Vector2 Max(Vector2 a, Vector2 b) {
        return new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }
    public static Vector2 Min(Vector2 a, Vector2 b) {
        return new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
    }
    
    public bool Equals(Vector2? other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return this == other;
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;

        return obj.GetType() == typeof(Vector2) && this == (Vector2) obj;
    }

    public override int GetHashCode() {
        return HashCode.Combine(X, Y);
    }

    public static Vector2 Zero => new(0, 0);
    public static Vector2 One => new(1, 1);
    public static Vector2 Up => new(0, 1);
    public static Vector2 Down => new(0, -1);
    public static Vector2 Left => new(-1, 0);
    public static Vector2 Right => new(1, 0);
}