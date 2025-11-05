using System;

namespace SDF;

/// <summary>
/// Simple 3D vector structure for SDF operations
/// </summary>
public struct Vector3
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Vector3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static readonly Vector3 Zero = new(0, 0, 0);
    public static readonly Vector3 One = new(1, 1, 1);
    public static readonly Vector3 UnitX = new(1, 0, 0);
    public static readonly Vector3 UnitY = new(0, 1, 0);
    public static readonly Vector3 UnitZ = new(0, 0, 1);

    public static Vector3 operator +(Vector3 a, Vector3 b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3 operator -(Vector3 a, Vector3 b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3 operator -(Vector3 a) =>
        new(-a.X, -a.Y, -a.Z);

    public static Vector3 operator *(Vector3 a, double scalar) =>
        new(a.X * scalar, a.Y * scalar, a.Z * scalar);

    public static Vector3 operator *(double scalar, Vector3 a) =>
        new(a.X * scalar, a.Y * scalar, a.Z * scalar);

    public static Vector3 operator /(Vector3 a, double scalar)
    {
        if (Math.Abs(scalar) < double.Epsilon)
            throw new DivideByZeroException("Cannot divide vector by zero");
        return new(a.X / scalar, a.Y / scalar, a.Z / scalar);
    }

    public double Length() =>
        Math.Sqrt(X * X + Y * Y + Z * Z);

    public double LengthSquared() =>
        X * X + Y * Y + Z * Z;

    public Vector3 Normalize()
    {
        var length = Length();
        return length > 0 ? this / length : Zero;
    }

    public static double Dot(Vector3 a, Vector3 b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Cross(Vector3 a, Vector3 b) =>
        new(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );

    public override string ToString() =>
        $"({X:F3}, {Y:F3}, {Z:F3})";
}
