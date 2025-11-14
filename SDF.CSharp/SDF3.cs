using System;
using System.Numerics;

namespace SDF;

/// <summary>
/// Represents a 3D Signed Distance Function
/// </summary>
public class SDF3
{
    private readonly Func<Vector3[], double[]> _function;
    internal double? SmoothingK { get; set; }

    public SDF3(Func<Vector3[], double[]> function)
    {
        _function = function ?? throw new ArgumentNullException(nameof(function));
    }

    /// <summary>
    /// Evaluate the SDF at given points
    /// </summary>
    public double[] Evaluate(Vector3[] points)
    {
        return _function(points);
    }

    /// <summary>
    /// Set smoothing factor for boolean operations
    /// </summary>
    public SDF3 K(double k)
    {
        SmoothingK = k;
        return this;
    }

    /// <summary>
    /// Union operation (OR)
    /// </summary>
    public static SDF3 operator |(SDF3 a, SDF3 b) =>
        Operations.Union(a, b);

    /// <summary>
    /// Intersection operation (AND)
    /// </summary>
    public static SDF3 operator &(SDF3 a, SDF3 b) =>
        Operations.Intersection(a, b);

    /// <summary>
    /// Difference operation (subtraction)
    /// </summary>
    public static SDF3 operator -(SDF3 a, SDF3 b) =>
        Operations.Difference(a, b);

    /// <summary>
    /// Generate mesh from this SDF
    /// </summary>
    public Vector3[] Generate(
        double? step = null,
        (Vector3 min, Vector3 max)? bounds = null,
        int samples = 1 << 22,
        int batchSize = 32,
        bool sparse = true,
        bool verbose = true)
    {
        return Core.Generate(this, step, bounds, samples, batchSize, sparse, verbose);
    }

    /// <summary>
    /// Save mesh to STL file
    /// </summary>
    public void Save(
        string path,
        double? step = null,
        (Vector3 min, Vector3 max)? bounds = null,
        int samples = 1 << 22,
        int batchSize = 32,
        bool sparse = true,
        bool verbose = true)
    {
        var points = Generate(step, bounds, samples, batchSize, sparse, verbose);
        StlWriter.WriteBinaryStl(path, points);
    }
}

