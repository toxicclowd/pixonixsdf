using System;
using System.Collections.Generic;
using System.Numerics;

namespace SDF;

/// <summary>
/// Extension methods for SDF3 to provide a fluent API
/// </summary>
public static class SDF3Extensions
{
    /// <summary>
    /// Generate a mesh from this SDF
    /// </summary>
    public static List<Vector3> Generate(this SDF3 sdf, 
        double? step = null, 
        (Vector3, Vector3)? bounds = null,
        int? samples = null,
        int? workers = null,
        int? batchSize = null,
        bool? verbose = null,
        bool? sparse = null)
    {
        var generator = new MeshGenerator();
        
        if (samples.HasValue) generator.Samples = samples.Value;
        if (workers.HasValue) generator.Workers = workers.Value;
        if (batchSize.HasValue) generator.BatchSize = batchSize.Value;
        if (verbose.HasValue) generator.Verbose = verbose.Value;
        if (sparse.HasValue) generator.Sparse = sparse.Value;
        
        return generator.Generate(sdf, step, bounds);
    }

    /// <summary>
    /// Save this SDF as an STL file
    /// </summary>
    public static void Save(this SDF3 sdf, string path,
        double? step = null,
        (Vector3, Vector3)? bounds = null,
        int? samples = null,
        int? workers = null,
        int? batchSize = null,
        bool? verbose = null,
        bool? sparse = null)
    {
        var triangles = sdf.Generate(step, bounds, samples, workers, batchSize, verbose, sparse);
        StlWriter.WriteBinaryStl(path, triangles);
        
        if (verbose ?? true)
        {
            Console.WriteLine($"Saved to {path}");
        }
    }
}
