using System;
using System.Numerics;

namespace SDF;

/// <summary>
/// Operations for transforming and combining SDFs
/// </summary>
public static class Operations
{
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
            var scaledPoints = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                scaledPoints[i] = points[i] / (float)factor;
            }
            var values = sdf.Evaluate(scaledPoints);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] *= factor;
            }
            return values;
        });
    }

    /// <summary>
    /// Rotate an SDF around an axis
    /// </summary>
    public static SDF3 Rotate(this SDF3 sdf, double angle, Vector3? axis = null)
    {
        var rotationAxis = axis ?? Constants.Z;
        var q = Quaternion.CreateFromAxisAngle(rotationAxis, (float)angle);
        var qInv = Quaternion.Conjugate(q);
        
        return new SDF3(points =>
        {
            var rotatedPoints = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                rotatedPoints[i] = Vector3.Transform(points[i], qInv);
            }
            return sdf.Evaluate(rotatedPoints);
        });
    }

    /// <summary>
    /// Orient an SDF to point in a specific direction
    /// </summary>
    public static SDF3 Orient(this SDF3 sdf, Vector3 direction)
    {
        var from = Constants.Z;
        var to = Vector3.Normalize(direction);
        
        // Calculate rotation needed to go from Z to the target direction
        var axis = Vector3.Cross(from, to);
        if (axis.LengthSquared() < 1e-6)
        {
            // Vectors are parallel or anti-parallel
            return Vector3.Dot(from, to) > 0 ? sdf : sdf.Rotate(Math.PI, Constants.X);
        }
        
        axis = Vector3.Normalize(axis);
        var angle = Math.Acos(Vector3.Dot(from, to));
        
        return sdf.Rotate(angle, axis);
    }

    // Boolean Operations
    
    /// <summary>
    /// Union of two SDFs
    /// </summary>
    public static SDF3 Union(SDF3 a, SDF3 b, double? k = null)
    {
        k ??= b.GetK();
        
        if (k.HasValue && k.Value > 0)
        {
            return new SDF3(points =>
            {
                var va = a.Evaluate(points);
                var vb = b.Evaluate(points);
                var result = new double[points.Length];
                
                for (int i = 0; i < points.Length; i++)
                {
                    result[i] = SmoothMin(va[i], vb[i], k.Value);
                }
                return result;
            });
        }
        
        return new SDF3(points =>
        {
            var va = a.Evaluate(points);
            var vb = b.Evaluate(points);
            var result = new double[points.Length];
            
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = Math.Min(va[i], vb[i]);
            }
            return result;
        });
    }

    /// <summary>
    /// Difference of two SDFs (a - b)
    /// </summary>
    public static SDF3 Difference(SDF3 a, SDF3 b, double? k = null)
    {
        k ??= b.GetK();
        
        if (k.HasValue && k.Value > 0)
        {
            return new SDF3(points =>
            {
                var va = a.Evaluate(points);
                var vb = b.Evaluate(points);
                var result = new double[points.Length];
                
                for (int i = 0; i < points.Length; i++)
                {
                    result[i] = SmoothMax(va[i], -vb[i], k.Value);
                }
                return result;
            });
        }
        
        return new SDF3(points =>
        {
            var va = a.Evaluate(points);
            var vb = b.Evaluate(points);
            var result = new double[points.Length];
            
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = Math.Max(va[i], -vb[i]);
            }
            return result;
        });
    }

    /// <summary>
    /// Intersection of two SDFs
    /// </summary>
    public static SDF3 Intersection(SDF3 a, SDF3 b, double? k = null)
    {
        k ??= b.GetK();
        
        if (k.HasValue && k.Value > 0)
        {
            return new SDF3(points =>
            {
                var va = a.Evaluate(points);
                var vb = b.Evaluate(points);
                var result = new double[points.Length];
                
                for (int i = 0; i < points.Length; i++)
                {
                    result[i] = SmoothMax(va[i], vb[i], k.Value);
                }
                return result;
            });
        }
        
        return new SDF3(points =>
        {
            var va = a.Evaluate(points);
            var vb = b.Evaluate(points);
            var result = new double[points.Length];
            
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = Math.Max(va[i], vb[i]);
            }
            return result;
        });
    }

    // Helper functions for smooth operations
    
    private static double SmoothMin(double a, double b, double k)
    {
        var h = Math.Max(k - Math.Abs(a - b), 0.0) / k;
        return Math.Min(a, b) - h * h * k * 0.25;
    }

    private static double SmoothMax(double a, double b, double k)
    {
        return -SmoothMin(-a, -b, k);
    }

    // Deformation Operations

    /// <summary>
    /// Twist an SDF around the Z axis
    /// </summary>
    public static SDF3 Twist(this SDF3 sdf, double k)
    {
        return new SDF3(points =>
        {
            var twistedPoints = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var c = Math.Cos(k * p.Z);
                var s = Math.Sin(k * p.Z);
                twistedPoints[i] = new Vector3(
                    (float)(c * p.X - s * p.Y),
                    (float)(s * p.X + c * p.Y),
                    p.Z
                );
            }
            return sdf.Evaluate(twistedPoints);
        });
    }

    /// <summary>
    /// Bend an SDF
    /// </summary>
    public static SDF3 Bend(this SDF3 sdf, double k)
    {
        return new SDF3(points =>
        {
            var bentPoints = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                var c = Math.Cos(k * p.X);
                var s = Math.Sin(k * p.X);
                bentPoints[i] = new Vector3(
                    (float)(c * p.X - s * p.Y),
                    (float)(s * p.X + c * p.Y),
                    p.Z
                );
            }
            return sdf.Evaluate(bentPoints);
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
                elongatedPoints[i] = new Vector3(
                    p.X - Math.Clamp(p.X, -size.X, size.X),
                    p.Y - Math.Clamp(p.Y, -size.Y, size.Y),
                    p.Z - Math.Clamp(p.Z, -size.Z, size.Z)
                );
            }
            return sdf.Evaluate(elongatedPoints);
        });
    }

    /// <summary>
    /// Dilate (expand) an SDF
    /// </summary>
    public static SDF3 Dilate(this SDF3 sdf, double r)
    {
        return new SDF3(points =>
        {
            var values = sdf.Evaluate(points);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] -= r;
            }
            return values;
        });
    }

    /// <summary>
    /// Erode (shrink) an SDF
    /// </summary>
    public static SDF3 Erode(this SDF3 sdf, double r)
    {
        return new SDF3(points =>
        {
            var values = sdf.Evaluate(points);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] += r;
            }
            return values;
        });
    }

    /// <summary>
    /// Create a shell of specified thickness
    /// </summary>
    public static SDF3 Shell(this SDF3 sdf, double thickness)
    {
        return new SDF3(points =>
        {
            var values = sdf.Evaluate(points);
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = Math.Abs(values[i]) - thickness;
            }
            return values;
        });
    }

    /// <summary>
    /// Repeat an SDF with specified spacing
    /// </summary>
    public static SDF3 Repeat(this SDF3 sdf, Vector3 spacing, Vector3? count = null)
    {
        if (count.HasValue)
        {
            // Finite repetition
            var c = count.Value;
            return new SDF3(points =>
            {
                var repeatedPoints = new Vector3[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    var p = points[i];
                    repeatedPoints[i] = new Vector3(
                        p.X - spacing.X * (float)Math.Clamp(Math.Round(p.X / spacing.X), -c.X, c.X),
                        p.Y - spacing.Y * (float)Math.Clamp(Math.Round(p.Y / spacing.Y), -c.Y, c.Y),
                        p.Z - spacing.Z * (float)Math.Clamp(Math.Round(p.Z / spacing.Z), -c.Z, c.Z)
                    );
                }
                return sdf.Evaluate(repeatedPoints);
            });
        }
        else
        {
            // Infinite repetition
            return new SDF3(points =>
            {
                var repeatedPoints = new Vector3[points.Length];
                for (int i = 0; i < points.Length; i++)
                {
                    var p = points[i];
                    repeatedPoints[i] = new Vector3(
                        p.X - spacing.X * (float)Math.Round(p.X / spacing.X),
                        p.Y - spacing.Y * (float)Math.Round(p.Y / spacing.Y),
                        p.Z - spacing.Z * (float)Math.Round(p.Z / spacing.Z)
                    );
                }
                return sdf.Evaluate(repeatedPoints);
            });
        }
    }
}
