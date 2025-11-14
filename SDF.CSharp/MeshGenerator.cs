using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SDF;

/// <summary>
/// Core mesh generation engine
/// </summary>
public class MeshGenerator
{
    public int Workers { get; set; } = Environment.ProcessorCount;
    public int Samples { get; set; } = 1 << 22; // 2^22
    public int BatchSize { get; set; } = 32;
    public bool Sparse { get; set; } = true;
    public bool Verbose { get; set; } = true;

    /// <summary>
    /// Generate a mesh from an SDF
    /// </summary>
    public List<Vector3> Generate(SDF3 sdf, 
        double? step = null, 
        (Vector3, Vector3)? bounds = null)
    {
        var (min, max) = bounds ?? EstimateBounds(sdf);
        
        if (Verbose)
        {
            Console.WriteLine($"Bounds: ({min.X:F2}, {min.Y:F2}, {min.Z:F2}) to ({max.X:F2}, {max.Y:F2}, {max.Z:F2})");
        }

        // Calculate step size if not provided
        if (!step.HasValue)
        {
            var volume = (max.X - min.X) * (max.Y - min.Y) * (max.Z - min.Z);
            step = Math.Pow(volume / Samples, 1.0 / 3.0);
        }

        if (Verbose)
        {
            Console.WriteLine($"Step size: {step:F6}");
        }

        // Calculate grid dimensions
        var nx = (int)Math.Ceiling((max.X - min.X) / step.Value);
        var ny = (int)Math.Ceiling((max.Y - min.Y) / step.Value);
        var nz = (int)Math.Ceiling((max.Z - min.Z) / step.Value);

        if (Verbose)
        {
            Console.WriteLine($"Grid dimensions: {nx} x {ny} x {nz}");
            Console.WriteLine("Generating mesh...");
        }

        // Create batches
        var batches = CreateBatches(min, max, step.Value, nx, ny, nz);
        
        if (Verbose)
        {
            Console.WriteLine($"Processing {batches.Count} batches...");
        }

        // Process batches in parallel
        var allTriangles = new List<Vector3>();
        var lockObj = new object();
        
        Parallel.ForEach(batches, new ParallelOptions { MaxDegreeOfParallelism = Workers }, batch =>
        {
            var triangles = ProcessBatch(sdf, batch);
            if (triangles != null && triangles.Count > 0)
            {
                lock (lockObj)
                {
                    allTriangles.AddRange(triangles);
                }
            }
        });

        if (Verbose)
        {
            Console.WriteLine($"Generated {allTriangles.Count / 3} triangles");
        }

        return allTriangles;
    }

    private List<Vector3>? ProcessBatch(SDF3 sdf, Batch batch)
    {
        // Skip batch if it's far from the surface (for sparse sampling)
        if (Sparse && ShouldSkip(sdf, batch))
        {
            return null;
        }

        // Sample the SDF at all grid points in the batch
        var points = new List<Vector3>();
        for (int x = 0; x < batch.Nx; x++)
        {
            for (int y = 0; y < batch.Ny; y++)
            {
                for (int z = 0; z < batch.Nz; z++)
                {
                    points.Add(new Vector3(
                        batch.MinX + x * batch.StepX,
                        batch.MinY + y * batch.StepY,
                        batch.MinZ + z * batch.StepZ
                    ));
                }
            }
        }

        var values = sdf.Evaluate(points.ToArray());
        
        // Reshape into 3D array
        var volume = new double[batch.Nx, batch.Ny, batch.Nz];
        int idx = 0;
        for (int x = 0; x < batch.Nx; x++)
        {
            for (int y = 0; y < batch.Ny; y++)
            {
                for (int z = 0; z < batch.Nz; z++)
                {
                    volume[x, y, z] = values[idx++];
                }
            }
        }

        // Apply marching cubes
        var step = new Vector3(batch.StepX, batch.StepY, batch.StepZ);
        var offset = new Vector3(batch.MinX, batch.MinY, batch.MinZ);
        return MarchingCubes.Generate(volume, offset, step);
    }

    private bool ShouldSkip(SDF3 sdf, Batch batch)
    {
        // Check center point and corners
        var center = new Vector3(
            (batch.MinX + batch.MaxX) / 2,
            (batch.MinY + batch.MaxY) / 2,
            (batch.MinZ + batch.MaxZ) / 2
        );

        var r = Math.Abs(sdf.Evaluate(new[] { center })[0]);
        var dx = batch.MaxX - batch.MinX;
        var dy = batch.MaxY - batch.MinY;
        var dz = batch.MaxZ - batch.MinZ;
        var d = Math.Sqrt(dx * dx + dy * dy + dz * dz) / 2;

        if (r > d)
        {
            // Check corners
            var corners = new[]
            {
                new Vector3(batch.MinX, batch.MinY, batch.MinZ),
                new Vector3(batch.MaxX, batch.MinY, batch.MinZ),
                new Vector3(batch.MinX, batch.MaxY, batch.MinZ),
                new Vector3(batch.MaxX, batch.MaxY, batch.MinZ),
                new Vector3(batch.MinX, batch.MinY, batch.MaxZ),
                new Vector3(batch.MaxX, batch.MinY, batch.MaxZ),
                new Vector3(batch.MinX, batch.MaxY, batch.MaxZ),
                new Vector3(batch.MaxX, batch.MaxY, batch.MaxZ),
            };

            var cornerValues = sdf.Evaluate(corners);
            var allPositive = cornerValues.All(v => v > 0);
            var allNegative = cornerValues.All(v => v < 0);
            
            return allPositive || allNegative;
        }

        return false;
    }

    private List<Batch> CreateBatches(Vector3 min, Vector3 max, double step, int nx, int ny, int nz)
    {
        var batches = new List<Batch>();
        var batchSize = BatchSize;

        for (int bx = 0; bx < nx; bx += batchSize)
        {
            for (int by = 0; by < ny; by += batchSize)
            {
                for (int bz = 0; bz < nz; bz += batchSize)
                {
                    var batch = new Batch
                    {
                        MinX = (float)(min.X + bx * step),
                        MinY = (float)(min.Y + by * step),
                        MinZ = (float)(min.Z + bz * step),
                        Nx = Math.Min(batchSize + 1, nx - bx + 1),
                        Ny = Math.Min(batchSize + 1, ny - by + 1),
                        Nz = Math.Min(batchSize + 1, nz - bz + 1),
                        StepX = (float)step,
                        StepY = (float)step,
                        StepZ = (float)step
                    };
                    batch.MaxX = (float)(batch.MinX + (batch.Nx - 1) * step);
                    batch.MaxY = (float)(batch.MinY + (batch.Ny - 1) * step);
                    batch.MaxZ = (float)(batch.MinZ + (batch.Nz - 1) * step);
                    
                    batches.Add(batch);
                }
            }
        }

        return batches;
    }

    public (Vector3, Vector3) EstimateBounds(SDF3 sdf)
    {
        const int samples = 16;
        var min = new Vector3(-1e9f, -1e9f, -1e9f);
        var max = new Vector3(1e9f, 1e9f, 1e9f);
        double prevThreshold = -1;

        for (int iteration = 0; iteration < 32; iteration++)
        {
            var dx = (max.X - min.X) / (samples - 1);
            var dy = (max.Y - min.Y) / (samples - 1);
            var dz = (max.Z - min.Z) / (samples - 1);
            
            var threshold = Math.Sqrt(dx * dx + dy * dy + dz * dz) / 2;
            
            if (Math.Abs(threshold - prevThreshold) < 1e-10)
                break;
            
            prevThreshold = threshold;

            var points = new List<Vector3>();
            for (int x = 0; x < samples; x++)
            {
                for (int y = 0; y < samples; y++)
                {
                    for (int z = 0; z < samples; z++)
                    {
                        points.Add(new Vector3(
                            min.X + x * dx,
                            min.Y + y * dy,
                            min.Z + z * dz
                        ));
                    }
                }
            }

            var values = sdf.Evaluate(points.ToArray());
            var nearSurface = new List<Vector3>();
            
            for (int i = 0; i < points.Count; i++)
            {
                if (Math.Abs(values[i]) <= threshold)
                {
                    nearSurface.Add(points[i]);
                }
            }

            if (nearSurface.Count == 0)
                break;

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

            min = newMin - new Vector3(dx, dy, dz) / 2;
            max = newMax + new Vector3(dx, dy, dz) / 2;
        }

        return (min, max);
    }

    private class Batch
    {
        public float MinX, MinY, MinZ;
        public float MaxX, MaxY, MaxZ;
        public int Nx, Ny, Nz;
        public float StepX, StepY, StepZ;
    }
}
