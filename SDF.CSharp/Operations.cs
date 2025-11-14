using System;
using System.Numerics;

namespace SDF;

/// <summary>
/// Operations for transforming and combining SDFs
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
        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var h = Math.Max(k - Math.Abs(da[i] - db[i]), 0.0) / k;
                result[i] = Math.Min(da[i], db[i]) - h * h * k * (1.0 / 4.0);
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
        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var h = Math.Max(k - Math.Abs(-db[i] - da[i]), 0.0) / k;
                result[i] = Math.Max(-db[i], da[i]) + h * h * k * (1.0 / 4.0);
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
        return new SDF3(points =>
        {
            var da = a.Evaluate(points);
            var db = b.Evaluate(points);
            var result = new double[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var h = Math.Max(k - Math.Abs(da[i] - db[i]), 0.0) / k;
                result[i] = Math.Max(da[i], db[i]) + h * h * k * (1.0 / 4.0);
            }
            return result;
        });
    }

    /// <summary>
    /// Translate (move) an SDF
    /// </summary>
    public static SDF3 Translate(this SDF3 sdf, Vector3 offset)
    {
        return new SDF3(points =>
        {
            var translatedPoints = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                translatedPoints[i] = points[i] - offset;
            }
            return sdf.Evaluate(translatedPoints);
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
                scaled[i] = points[i] / (float)factor;
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
        var rotAxis = axis ?? Constants.Z;
        var normalized = Vector3.Normalize(rotAxis);
        var cos = (float)Math.Cos(angle);
        var sin = (float)Math.Sin(angle);

        return new SDF3(points =>
        {
            var rotated = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var dot = Vector3.Dot(p, normalized);
                var cross = Vector3.Cross(normalized, p);
                rotated[i] = normalized * dot + cross * sin + (p - normalized * dot) * cos;
            }
            return sdf.Evaluate(rotated);
        });
    }

    /// <summary>
    /// Orient an SDF to point in a specific direction (rotates from Z-axis)
    /// </summary>
    public static SDF3 Orient(this SDF3 sdf, Vector3 direction)
    {
        var normalized = Vector3.Normalize(direction);
        var z = Constants.Z;

        if (Math.Abs(Vector3.Dot(normalized, z) - 1.0f) < 0.0001f)
        {
            return sdf;
        }
        if (Math.Abs(Vector3.Dot(normalized, z) + 1.0f) < 0.0001f)
        {
            return sdf.Rotate(Math.PI, Constants.X);
        }

        var axis = Vector3.Normalize(Vector3.Cross(z, normalized));
        var angle = Math.Acos(Vector3.Dot(z, normalized));
        return sdf.Rotate(angle, axis);
    }

    /// <summary>
    /// Twist an SDF around the Z axis
    /// </summary>
    public static SDF3 Twist(this SDF3 sdf, double k)
    {
        return new SDF3(points =>
        {
            var twisted = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var c = (float)Math.Cos(k * p.Z);
                var s = (float)Math.Sin(k * p.Z);
                twisted[i] = new Vector3(c * p.X - s * p.Y, s * p.X + c * p.Y, p.Z);
            }
            return sdf.Evaluate(twisted);
        });
    }

    /// <summary>
    /// Bend an SDF
    /// </summary>
    public static SDF3 Bend(this SDF3 sdf, double k)
    {
        return new SDF3(points =>
        {
            var bent = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var c = (float)Math.Cos(k * p.X);
                var s = (float)Math.Sin(k * p.X);
                bent[i] = new Vector3(c * p.X - s * p.Y, s * p.X + c * p.Y, p.Z);
            }
            return sdf.Evaluate(bent);
        });
    }

    /// <summary>
    /// Elongate an SDF along each axis
    /// </summary>
    public static SDF3 Elongate(this SDF3 sdf, Vector3 size)
    {
        return new SDF3(points =>
        {
            var elongatedPoints = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var q = new Vector3(
                    Math.Abs(p.X) - size.X,
                    Math.Abs(p.Y) - size.Y,
                    Math.Abs(p.Z) - size.Z
                );
                elongatedPoints[i] = new Vector3(
                    Math.Sign(p.X) * Math.Max(q.X, 0),
                    Math.Sign(p.Y) * Math.Max(q.Y, 0),
                    Math.Sign(p.Z) * Math.Max(q.Z, 0)
                );
            }
            var result = sdf.Evaluate(elongatedPoints);
            for (int i = 0; i < result.Length; i++)
            {
                var p = points[i];
                var q = new Vector3(
                    Math.Abs(p.X) - size.X,
                    Math.Abs(p.Y) - size.Y,
                    Math.Abs(p.Z) - size.Z
                );
                var outside = new Vector3(
                    Math.Max(q.X, 0),
                    Math.Max(q.Y, 0),
                    Math.Max(q.Z, 0)
                ).Length();
                result[i] += outside;
            }
            return result;
        });
    }

    /// <summary>
    /// Dilate (expand) an SDF
    /// </summary>
    public static SDF3 Dilate(this SDF3 sdf, double r)
    {
        return new SDF3(points =>
        {
            var result = sdf.Evaluate(points);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] -= r;
            }
            return result;
        });
    }

    /// <summary>
    /// Erode (shrink) an SDF
    /// </summary>
    public static SDF3 Erode(this SDF3 sdf, double r)
    {
        return new SDF3(points =>
        {
            var result = sdf.Evaluate(points);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] += r;
            }
            return result;
        });
    }

    /// <summary>
    /// Create a shell of specified thickness
    /// </summary>
    public static SDF3 Shell(this SDF3 sdf, double thickness)
    {
        return new SDF3(points =>
        {
            var result = sdf.Evaluate(points);
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Math.Abs(result[i]) - thickness;
            }
            return result;
        });
    }

    /// <summary>
    /// Repeat an SDF with specified spacing
    /// </summary>
    public static SDF3 Repeat(this SDF3 sdf, Vector3 spacing, Vector3? count = null)
    {
        return new SDF3(points =>
        {
            var repeated = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                if (count.HasValue)
                {
                    var c = count.Value;
                    var q = new Vector3(
                        p.X - spacing.X * (float)Math.Round(Math.Clamp(p.X / spacing.X, -c.X, c.X)),
                        p.Y - spacing.Y * (float)Math.Round(Math.Clamp(p.Y / spacing.Y, -c.Y, c.Y)),
                        p.Z - spacing.Z * (float)Math.Round(Math.Clamp(p.Z / spacing.Z, -c.Z, c.Z))
                    );
                    repeated[i] = q;
                }
                else
                {
                    repeated[i] = new Vector3(
                        p.X - spacing.X * (float)Math.Round(p.X / spacing.X),
                        p.Y - spacing.Y * (float)Math.Round(p.Y / spacing.Y),
                        p.Z - spacing.Z * (float)Math.Round(p.Z / spacing.Z)
                    );
                }
            }
            return sdf.Evaluate(repeated);
        });
    }
}
