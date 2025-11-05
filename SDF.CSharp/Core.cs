using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDF;

/// <summary>
/// Core mesh generation engine
/// </summary>
public static class Core
{
    /// <summary>
    /// Generate mesh from SDF
    /// </summary>
    public static Vector3[] Generate(
        SDF3 sdf,
        double? step = null,
        (Vector3 min, Vector3 max)? bounds = null,
        int samples = 1 << 22,
        int batchSize = 32,
        bool sparse = true,
        bool verbose = true)
    {
        var startTime = DateTime.Now;

        // Estimate bounds if not provided
        if (!bounds.HasValue)
        {
            bounds = EstimateBounds(sdf);
            if (verbose)
            {
                Console.WriteLine($"Estimated bounds: {bounds.Value.min} to {bounds.Value.max}");
            }
        }

        var (min, max) = bounds.Value;

        // Calculate step size if not provided
        if (!step.HasValue)
        {
            var volume = (max.X - min.X) * (max.Y - min.Y) * (max.Z - min.Z);
            step = Math.Pow(volume / samples, 1.0 / 3.0);
        }

        var stepValue = step.Value;

        // Calculate grid dimensions
        var nx = (int)Math.Ceiling((max.X - min.X) / stepValue) + 1;
        var ny = (int)Math.Ceiling((max.Y - min.Y) / stepValue) + 1;
        var nz = (int)Math.Ceiling((max.Z - min.Z) / stepValue) + 1;

        if (verbose)
        {
            Console.WriteLine($"Grid size: {nx} x {ny} x {nz} = {nx * ny * nz} points");
            Console.WriteLine($"Step size: {stepValue:F6}");
        }

        // Generate batches
        var batches = GenerateBatches(min, max, stepValue, batchSize);
        var allTriangles = new List<Vector3>();
        int processed = 0;

        // Process batches in parallel
        var results = new List<Vector3>[batches.Count];
        
        Parallel.For(0, batches.Count, i =>
        {
            var batch = batches[i];
            results[i] = ProcessBatch(sdf, batch, stepValue, sparse);
            
            if (verbose)
            {
                lock (allTriangles)
                {
                    processed++;
                    if (processed % 100 == 0 || processed == batches.Count)
                    {
                        var progress = (double)processed / batches.Count * 100.0;
                        Console.Write($"\rProgress: {progress:F1}% ({processed}/{batches.Count} batches)");
                    }
                }
            }
        });

        // Combine results
        foreach (var result in results)
        {
            if (result != null && result.Count > 0)
            {
                allTriangles.AddRange(result);
            }
        }

        if (verbose)
        {
            Console.WriteLine();
            var elapsed = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine($"Generated {allTriangles.Count / 3} triangles in {elapsed:F2}s");
        }

        return allTriangles.ToArray();
    }

    private static List<(Vector3 min, Vector3 max)> GenerateBatches(
        Vector3 min, Vector3 max, double step, int batchSize)
    {
        var batches = new List<(Vector3, Vector3)>();
        var batchStep = step * batchSize;

        for (double x = min.X; x < max.X; x += batchStep)
        {
            for (double y = min.Y; y < max.Y; y += batchStep)
            {
                for (double z = min.Z; z < max.Z; z += batchStep)
                {
                    var batchMin = new Vector3(x, y, z);
                    var batchMax = new Vector3(
                        Math.Min(x + batchStep, max.X),
                        Math.Min(y + batchStep, max.Y),
                        Math.Min(z + batchStep, max.Z)
                    );
                    batches.Add((batchMin, batchMax));
                }
            }
        }

        return batches;
    }

    private static List<Vector3> ProcessBatch(
        SDF3 sdf,
        (Vector3 min, Vector3 max) bounds,
        double step,
        bool sparse)
    {
        var (min, max) = bounds;

        // Check if we can skip this batch (sparse sampling)
        if (sparse && CanSkipBatch(sdf, min, max))
        {
            return new List<Vector3>();
        }

        // Generate grid points
        var nx = (int)Math.Ceiling((max.X - min.X) / step) + 1;
        var ny = (int)Math.Ceiling((max.Y - min.Y) / step) + 1;
        var nz = (int)Math.Ceiling((max.Z - min.Z) / step) + 1;

        var points = new List<Vector3>();
        for (int ix = 0; ix < nx; ix++)
        {
            for (int iy = 0; iy < ny; iy++)
            {
                for (int iz = 0; iz < nz; iz++)
                {
                    points.Add(new Vector3(
                        min.X + ix * step,
                        min.Y + iy * step,
                        min.Z + iz * step
                    ));
                }
            }
        }

        // Evaluate SDF
        var values = sdf.Evaluate(points.ToArray());

        // Create 3D volume
        var volume = new double[nx, ny, nz];
        int idx = 0;
        for (int ix = 0; ix < nx; ix++)
        {
            for (int iy = 0; iy < ny; iy++)
            {
                for (int iz = 0; iz < nz; iz++)
                {
                    volume[ix, iy, iz] = values[idx++];
                }
            }
        }

        // Apply marching cubes
        var scale = new Vector3(step, step, step);
        return MarchingCubes.Generate(volume, min, scale);
    }

    private static bool CanSkipBatch(SDF3 sdf, Vector3 min, Vector3 max)
    {
        // Sample center point
        var center = (min + max) * 0.5;
        var centerDist = sdf.Evaluate(new[] { center })[0];
        
        // Calculate maximum distance from center to corner
        var diagonal = (max - min).Length() * 0.5;

        // If the surface is definitely not in this batch, skip it
        if (Math.Abs(centerDist) > diagonal)
        {
            // Sample corners to verify
            var corners = new[]
            {
                new Vector3(min.X, min.Y, min.Z),
                new Vector3(max.X, min.Y, min.Z),
                new Vector3(min.X, max.Y, min.Z),
                new Vector3(max.X, max.Y, min.Z),
                new Vector3(min.X, min.Y, max.Z),
                new Vector3(max.X, min.Y, max.Z),
                new Vector3(min.X, max.Y, max.Z),
                new Vector3(max.X, max.Y, max.Z)
            };

            var cornerValues = sdf.Evaluate(corners);
            var allPositive = cornerValues.All(v => v > 0);
            var allNegative = cornerValues.All(v => v < 0);

            return allPositive || allNegative;
        }

        return false;
    }

    /// <summary>
    /// Estimate the bounding box of an SDF
    /// </summary>
    public static (Vector3 min, Vector3 max) EstimateBounds(SDF3 sdf)
    {
        const int samples = 16;
        var min = new Vector3(-1e9, -1e9, -1e9);
        var max = new Vector3(1e9, 1e9, 1e9);

        for (int iteration = 0; iteration < 32; iteration++)
        {
            var points = new List<Vector3>();
            var stepX = (max.X - min.X) / (samples - 1);
            var stepY = (max.Y - min.Y) / (samples - 1);
            var stepZ = (max.Z - min.Z) / (samples - 1);

            for (int ix = 0; ix < samples; ix++)
            {
                for (int iy = 0; iy < samples; iy++)
                {
                    for (int iz = 0; iz < samples; iz++)
                    {
                        points.Add(new Vector3(
                            min.X + ix * stepX,
                            min.Y + iy * stepY,
                            min.Z + iz * stepZ
                        ));
                    }
                }
            }

            var values = sdf.Evaluate(points.ToArray());
            var diagonal = (max - min).Length() / samples;
            var threshold = diagonal * 0.5;

            // Find points near the surface
            var nearSurface = new List<Vector3>();
            for (int i = 0; i < points.Count; i++)
            {
                if (Math.Abs(values[i]) <= threshold)
                {
                    nearSurface.Add(points[i]);
                }
            }

            if (nearSurface.Count == 0)
            {
                break;
            }

            // Update bounds to encompass surface points
            var newMin = nearSurface[0];
            var newMax = nearSurface[0];

            foreach (var p in nearSurface)
            {
                newMin = new Vector3(
                    Math.Min(newMin.X, p.X),
                    Math.Min(newMin.Y, p.Y),
                    Math.Min(newMin.Z, p.Z)
                );
                newMax = new Vector3(
                    Math.Max(newMax.X, p.X),
                    Math.Max(newMax.Y, p.Y),
                    Math.Max(newMax.Z, p.Z)
                );
            }

            // Add padding
            var padding = new Vector3(stepX, stepY, stepZ);
            min = newMin - padding;
            max = newMax + padding;

            // Check for convergence
            if (diagonal < 1e-6)
            {
                break;
            }
        }

        return (min, max);
    }
}
