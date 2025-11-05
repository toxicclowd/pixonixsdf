using System;

namespace SDF;

/// <summary>
/// Boolean operations and transformations for SDFs
/// </summary>
public static class Operations
{
    /// <summary>
    /// Union of two SDFs (minimum)
    /// </summary>
    public static SDF3 Union(SDF3 a, SDF3 b, double? k = null)
    {
        k ??= b.SmoothingK ?? a.SmoothingK;

        if (k.HasValue && k.Value > 0)
        {
            return SmoothUnion(a, b, k.Value);
        }

        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = Math.Min(da[i], db[i]);
            }
            return result;
        });
    }

    /// <summary>
    /// Smooth union of two SDFs
    /// </summary>
    public static SDF3 SmoothUnion(SDF3 a, SDF3 b, double k)
    {
        if (k <= 0)
            throw new ArgumentException("Smoothing factor k must be positive", nameof(k));

        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var h = Math.Max(k - Math.Abs(da[i] - db[i]), 0.0) / k;
                result[i] = Math.Min(da[i], db[i]) - h * h * k * 0.25;
            }
            return result;
        });
    }

    /// <summary>
    /// Difference of two SDFs (subtraction)
    /// </summary>
    public static SDF3 Difference(SDF3 a, SDF3 b, double? k = null)
    {
        k ??= b.SmoothingK ?? a.SmoothingK;

        if (k.HasValue && k.Value > 0)
        {
            return SmoothDifference(a, b, k.Value);
        }

        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = Math.Max(da[i], -db[i]);
            }
            return result;
        });
    }

    /// <summary>
    /// Smooth difference of two SDFs
    /// </summary>
    public static SDF3 SmoothDifference(SDF3 a, SDF3 b, double k)
    {
        if (k <= 0)
            throw new ArgumentException("Smoothing factor k must be positive", nameof(k));

        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var h = Math.Max(k - Math.Abs(da[i] + db[i]), 0.0) / k;
                result[i] = Math.Max(da[i], -db[i]) + h * h * k * 0.25;
            }
            return result;
        });
    }

    /// <summary>
    /// Intersection of two SDFs (maximum)
    /// </summary>
    public static SDF3 Intersection(SDF3 a, SDF3 b, double? k = null)
    {
        k ??= b.SmoothingK ?? a.SmoothingK;

        if (k.HasValue && k.Value > 0)
        {
            return SmoothIntersection(a, b, k.Value);
        }

        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = Math.Max(da[i], db[i]);
            }
            return result;
        });
    }

    /// <summary>
    /// Smooth intersection of two SDFs
    /// </summary>
    public static SDF3 SmoothIntersection(SDF3 a, SDF3 b, double k)
    {
        if (k <= 0)
            throw new ArgumentException("Smoothing factor k must be positive", nameof(k));

        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var h = Math.Max(k - Math.Abs(da[i] - db[i]), 0.0) / k;
                result[i] = Math.Max(da[i], db[i]) + h * h * k * 0.25;
            }
            return result;
        });
    }

    /// <summary>
    /// Translate an SDF by an offset
    /// </summary>
    public static SDF3 Translate(this SDF3 sdf, Vector3 offset)
    {
        return new SDF3(points =>
        {
            var translated = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                translated[i] = points[i] - offset;
            }
            return sdf.Evaluate(translated);
        });
    }

    /// <summary>
    /// Scale an SDF uniformly
    /// </summary>
    public static SDF3 Scale(this SDF3 sdf, double factor)
    {
        return new SDF3(points =>
        {
            var scaled = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                scaled[i] = points[i] / factor;
            }
            var result = sdf.Evaluate(scaled);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] *= factor;
            }
            return result;
        });
    }

    /// <summary>
    /// Rotate an SDF around an axis
    /// </summary>
    public static SDF3 Rotate(this SDF3 sdf, double angle, Vector3? axis = null)
    {
        var a = (axis ?? Vector3.UnitZ).Normalize();
        var s = Math.Sin(-angle);
        var c = Math.Cos(-angle);
        var t = 1.0 - c;

        // Rotation matrix
        var m00 = t * a.X * a.X + c;
        var m01 = t * a.X * a.Y - s * a.Z;
        var m02 = t * a.X * a.Z + s * a.Y;
        var m10 = t * a.X * a.Y + s * a.Z;
        var m11 = t * a.Y * a.Y + c;
        var m12 = t * a.Y * a.Z - s * a.X;
        var m20 = t * a.X * a.Z - s * a.Y;
        var m21 = t * a.Y * a.Z + s * a.X;
        var m22 = t * a.Z * a.Z + c;

        return new SDF3(points =>
        {
            var rotated = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                rotated[i] = new Vector3(
                    m00 * p.X + m01 * p.Y + m02 * p.Z,
                    m10 * p.X + m11 * p.Y + m12 * p.Z,
                    m20 * p.X + m21 * p.Y + m22 * p.Z
                );
            }
            return sdf.Evaluate(rotated);
        });
    }

    /// <summary>
    /// Orient an SDF to point in a specific direction (rotates from Z-axis)
    /// </summary>
    public static SDF3 Orient(this SDF3 sdf, Vector3 direction)
    {
        var d = direction.Normalize();
        var z = Vector3.UnitZ;

        // If direction is already aligned with Z, no rotation needed
        if (Math.Abs(Vector3.Dot(d, z) - 1.0) < 1e-6)
        {
            return sdf;
        }

        // If direction is opposite to Z, rotate 180 degrees around X
        if (Math.Abs(Vector3.Dot(d, z) + 1.0) < 1e-6)
        {
            return sdf.Rotate(Math.PI, Vector3.UnitX);
        }

        // Find rotation axis and angle
        var axis = Vector3.Cross(z, d);
        var angle = Math.Acos(Vector3.Dot(z, d));

        return sdf.Rotate(angle, axis);
    }
}
