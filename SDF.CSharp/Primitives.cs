using System;
using System.Collections.Generic;
using System.Numerics;

namespace SDF;

/// <summary>
/// 3D primitive shapes as SDFs
/// </summary>
public static class Primitives
{
    /// <summary>
    /// Create a sphere SDF
    /// </summary>
    public static SDF3 Sphere(double radius = 1.0, Vector3? center = null)
    {
        var c = center ?? Constants.Origin;
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var diff = points[i] - c;
                result[i] = diff.Length() - radius;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a box SDF
    /// </summary>
    public static SDF3 Box(Vector3 size, Vector3? center = null)
    {
        var c = center ?? Constants.Origin;
        var halfSize = size / 2.0f;
        
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
                var outside = Vector3.Max(q, Vector3.Zero).Length();
                var inside = Math.Min(Math.Max(q.X, Math.Max(q.Y, q.Z)), 0.0);
                result[i] = outside + inside;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a box SDF with uniform size
    /// </summary>
    public static SDF3 Box(double size = 1.0, Vector3? center = null)
    {
        return Box(new Vector3(size, size, size), center);
    }

    /// <summary>
    /// Create an infinite cylinder along the Z axis
    /// </summary>
    public static SDF3 Cylinder(double radius)
    {
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var d = Math.Sqrt(p.X * p.X + p.Y * p.Y);
                result[i] = d - radius;
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
                var x = (pa * baba - ba * paba).Length() - radius * baba;
                var y = Math.Abs(paba - baba * 0.5) - baba * 0.5;
                var x2 = x * x;
                var y2 = y * y * baba;
                var d = (Math.Max(x, y) < 0) ? -Math.Min(x2, y2) : ((x > 0) ? x2 : 0) + ((y > 0) ? y2 : 0);
                result[i] = Math.Sign(d) * Math.Sqrt(Math.Abs(d)) / baba;
            }
            return result;
        });
    }

    /// <summary>
    /// Create an infinite plane
    /// </summary>
    public static SDF3 Plane(Vector3? normal = null, Vector3? point = null)
    {
        var n = Vector3.Normalize(normal ?? Constants.Up);
        var pt = point ?? Constants.Origin;
        
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = Vector3.Dot(points[i] - pt, n);
            }
            return result;
        });
    }

    /// <summary>
    /// Create a torus
    /// </summary>
    public static SDF3 Torus(double r1, double r2)
    {
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var qx = Math.Sqrt(p.X * p.X + p.Y * p.Y) - r1;
                var qy = p.Z;
                result[i] = Math.Sqrt(qx * qx + qy * qy) - r2;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a rounded box SDF
    /// </summary>
    public static SDF3 RoundedBox(Vector3 size, double radius)
    {
        var halfSize = size / 2.0 - new Vector3(radius, radius, radius);
        
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var q = new Vector3(
                    Math.Abs(p.X) - halfSize.X,
                    Math.Abs(p.Y) - halfSize.Y,
                    Math.Abs(p.Z) - halfSize.Z
                );
                var outside = Vector3.Max(q, Vector3.Zero).Length();
                var inside = Math.Min(Math.Max(q.X, Math.Max(q.Y, q.Z)), 0.0);
                result[i] = outside + inside - radius;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a capsule (cylinder with rounded ends) between two points
    /// </summary>
    public static SDF3 Capsule(Vector3 a, Vector3 b, double radius)
    {
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            var ba = b - a;
            var baba = Vector3.Dot(ba, ba);
            
            for (int i = 0; i < points.Length; i++)
            {
                var pa = points[i] - a;
                var h = Math.Clamp(Vector3.Dot(pa, ba) / baba, 0.0, 1.0);
                result[i] = (pa - ba * (float)h).Length() - radius;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a slab (infinite planes limiting space on certain axes)
    /// </summary>
    public static SDF3 Slab(double? x0 = null, double? y0 = null, double? z0 = null,
                            double? x1 = null, double? y1 = null, double? z1 = null)
    {
        var planes = new List<SDF3>();
        
        if (x0.HasValue) planes.Add(Plane(Constants.X, new Vector3((float)x0.Value, 0, 0)));
        if (x1.HasValue) planes.Add(Plane(-Constants.X, new Vector3((float)x1.Value, 0, 0)));
        if (y0.HasValue) planes.Add(Plane(Constants.Y, new Vector3(0, (float)y0.Value, 0)));
        if (y1.HasValue) planes.Add(Plane(-Constants.Y, new Vector3(0, (float)y1.Value, 0)));
        if (z0.HasValue) planes.Add(Plane(Constants.Z, new Vector3(0, 0, (float)z0.Value)));
        if (z1.HasValue) planes.Add(Plane(-Constants.Z, new Vector3(0, 0, (float)z1.Value)));
        
        if (planes.Count == 0)
            throw new ArgumentException("At least one plane must be specified");
        
        var result = planes[0];
        for (int i = 1; i < planes.Count; i++)
        {
            result = result & planes[i];
        }
        return result;
    }

    /// <summary>
    /// Create an ellipsoid with different radii on each axis
    /// </summary>
    public static SDF3 Ellipsoid(Vector3 size)
    {
        return new SDF3(points =>
        {
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var k0 = new Vector3(p.X / size.X, p.Y / size.Y, p.Z / size.Z).Length();
                var k1 = new Vector3(p.X / (size.X * size.X), p.Y / (size.Y * size.Y), p.Z / (size.Z * size.Z)).Length();
                result[i] = k0 * (k0 - 1.0) / k1;
            }
            return result;
        });
    }
}
