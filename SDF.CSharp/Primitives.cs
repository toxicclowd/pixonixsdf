using System;

namespace SDF;

/// <summary>
/// 3D SDF primitive shapes
/// </summary>
public static class Primitives
{
    /// <summary>
    /// Create a sphere SDF
    /// </summary>
    public static SDF3 Sphere(double radius = 1.0, Vector3? center = null)
    {
        var c = center ?? Vector3.Zero;
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i] - c;
                result[i] = p.Length() - radius;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a box SDF
    /// </summary>
    public static SDF3 Box(Vector3 size, Vector3? center = null)
    {
        var c = center ?? Vector3.Zero;
        var halfSize = size * 0.5;

        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i] - c;
                var q = new Vector3(
                    Math.Abs(p.X) - halfSize.X,
                    Math.Abs(p.Y) - halfSize.Y,
                    Math.Abs(p.Z) - halfSize.Z
                );
                var outside = new Vector3(
                    Math.Max(q.X, 0),
                    Math.Max(q.Y, 0),
                    Math.Max(q.Z, 0)
                ).Length();
                var inside = Math.Min(Math.Max(q.X, Math.Max(q.Y, q.Z)), 0.0);
                result[i] = outside + inside;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a box SDF with uniform size
    /// </summary>
    public static SDF3 Box(double size = 1.0, Vector3? center = null) =>
        Box(new Vector3(size, size, size), center);

    /// <summary>
    /// Create an infinite cylinder along Z axis
    /// </summary>
    public static SDF3 Cylinder(double radius)
    {
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                result[i] = Math.Sqrt(p.X * p.X + p.Y * p.Y) - radius;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a capped cylinder between two points
    /// </summary>
    public static SDF3 CappedCylinder(Vector3 a, Vector3 b, double radius)
    {
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            var ba = b - a;
            var baba = Vector3.Dot(ba, ba);

            for (int i = 0; i < points.Length; i++)
            {
                var pa = points[i] - a;
                var paba = Vector3.Dot(pa, ba);
                var x = (pa * baba - ba * paba).Length();
                var y = Math.Abs(paba - baba * 0.5) - baba * 0.5;
                var x2 = x * x;
                var y2 = y * y * baba;

                var d = (Math.Max(x, y) < 0.0)
                    ? -Math.Min(x2, y2)
                    : ((x > 0.0) ? x2 : 0.0) + ((y > 0.0) ? y2 : 0.0);

                result[i] = Math.Sign(d) * Math.Sqrt(Math.Abs(d)) / baba - radius;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a torus SDF
    /// </summary>
    public static SDF3 Torus(double r1, double r2)
    {
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var q = new Vector3(
                    Math.Sqrt(p.X * p.X + p.Y * p.Y) - r1,
                    0,
                    p.Z
                );
                result[i] = q.Length() - r2;
            }
            return result;
        });
    }

    /// <summary>
    /// Create an infinite plane SDF
    /// </summary>
    public static SDF3 Plane(Vector3? normal = null, Vector3? point = null)
    {
        var n = (normal ?? Vector3.UnitZ).Normalize();
        var p0 = point ?? Vector3.Zero;

        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = Vector3.Dot(points[i] - p0, n);
            }
            return result;
        });
    }
}
