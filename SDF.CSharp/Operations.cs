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
}
