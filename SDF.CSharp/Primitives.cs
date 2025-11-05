using System;
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
        return Box(new Vector3((float)size), center);
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
}
