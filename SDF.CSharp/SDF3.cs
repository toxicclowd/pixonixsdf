using System;
using System.Numerics;

namespace SDF;

/// <summary>
/// Represents a 3D Signed Distance Function
/// </summary>
public class SDF3
{
    private readonly Func<Vector3[], double[]> _function;
    private double? _k;

    public SDF3(Func<Vector3[], double[]> function)
    {
        _function = function;
    }

    /// <summary>
    /// Evaluate the SDF at given points
    /// </summary>
    public double[] Evaluate(Vector3[] points)
    {
        return _function(points);
    }

    /// <summary>
    /// Set smoothing parameter for boolean operations
    /// </summary>
    public SDF3 K(double k)
    {
        _k = k;
        return this;
    }

    internal double? GetK() => _k;

    // Boolean operations using operator overloading
    public static SDF3 operator |(SDF3 a, SDF3 b) => Operations.Union(a, b);
    public static SDF3 operator &(SDF3 a, SDF3 b) => Operations.Intersection(a, b);
    public static SDF3 operator -(SDF3 a, SDF3 b) => Operations.Difference(a, b);
}
