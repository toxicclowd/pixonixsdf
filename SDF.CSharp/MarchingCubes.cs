using System;
using System.Collections.Generic;

namespace SDF;

/// <summary>
/// Simplified marching cubes implementation for mesh generation
/// Uses a surface net approach for simplicity while maintaining quality
/// </summary>
public static class MarchingCubes
{
    /// <summary>
    /// Generate mesh triangles from a volume using a simplified surface extraction
    /// </summary>
    public static List<Vector3> Generate(double[,,] volume, Vector3 origin, Vector3 scale)
    {
        var vertices = new List<Vector3>();
        int sizeX = volume.GetLength(0);
        int sizeY = volume.GetLength(1);
        int sizeZ = volume.GetLength(2);

        // Use a surface nets approach - simpler than full marching cubes
        // but still produces good quality meshes
        for (int x = 0; x < sizeX - 1; x++)
        {
            for (int y = 0; y < sizeY - 1; y++)
            {
                for (int z = 0; z < sizeZ - 1; z++)
                {
                    // Check each face of the current cell for sign changes
                    var v000 = volume[x, y, z];
                    var v100 = volume[x + 1, y, z];
                    var v010 = volume[x, y + 1, z];
                    var v110 = volume[x + 1, y + 1, z];
                    var v001 = volume[x, y, z + 1];
                    var v101 = volume[x + 1, y, z + 1];
                    var v011 = volume[x, y + 1, z + 1];
                    var v111 = volume[x + 1, y + 1, z + 1];

                    // Count sign changes across the cube
                    int signChanges = 0;
                    if ((v000 < 0) != (v100 < 0)) signChanges++;
                    if ((v000 < 0) != (v010 < 0)) signChanges++;
                    if ((v000 < 0) != (v001 < 0)) signChanges++;
                    if ((v100 < 0) != (v110 < 0)) signChanges++;
                    if ((v100 < 0) != (v101 < 0)) signChanges++;
                    if ((v010 < 0) != (v110 < 0)) signChanges++;
                    if ((v010 < 0) != (v011 < 0)) signChanges++;
                    if ((v001 < 0) != (v101 < 0)) signChanges++;
                    if ((v001 < 0) != (v011 < 0)) signChanges++;
                    if ((v110 < 0) != (v111 < 0)) signChanges++;
                    if ((v101 < 0) != (v111 < 0)) signChanges++;
                    if ((v011 < 0) != (v111 < 0)) signChanges++;

                    if (signChanges == 0)
                        continue;

                    // Calculate surface position using interpolation
                    var surfacePos = new Vector3(x + 0.5, y + 0.5, z + 0.5);
                    
                    // Adjust position based on gradient (simple approximation)
                    double sumValues = v000 + v100 + v010 + v110 + v001 + v101 + v011 + v111;
                    double avgValue = sumValues / 8.0;
                    
                    if (Math.Abs(avgValue) > 1e-6)
                    {
                        var offset = 0.3 * avgValue; // Adjust toward surface
                        surfacePos = new Vector3(
                            x + 0.5 + offset,
                            y + 0.5 + offset,
                            z + 0.5 + offset
                        );
                    }

                    // Transform to world space
                    var worldPos = origin + new Vector3(
                        surfacePos.X * scale.X,
                        surfacePos.Y * scale.Y,
                        surfacePos.Z * scale.Z
                    );

                    // Generate triangles based on which edges have sign changes
                    // X-direction face
                    if ((v000 < 0) != (v100 < 0))
                    {
                        var p0 = Interpolate(origin + new Vector3(x * scale.X, y * scale.Y, z * scale.Z),
                                           origin + new Vector3((x + 1) * scale.X, y * scale.Y, z * scale.Z),
                                           v000, v100);
                        var p1 = Interpolate(origin + new Vector3(x * scale.X, (y + 1) * scale.Y, z * scale.Z),
                                           origin + new Vector3((x + 1) * scale.X, (y + 1) * scale.Y, z * scale.Z),
                                           v010, v110);
                        var p2 = Interpolate(origin + new Vector3(x * scale.X, y * scale.Y, (z + 1) * scale.Z),
                                           origin + new Vector3((x + 1) * scale.X, y * scale.Y, (z + 1) * scale.Z),
                                           v001, v101);

                        vertices.Add(p0);
                        vertices.Add(p1);
                        vertices.Add(p2);
                    }

                    // Y-direction face
                    if ((v000 < 0) != (v010 < 0))
                    {
                        var p0 = Interpolate(origin + new Vector3(x * scale.X, y * scale.Y, z * scale.Z),
                                           origin + new Vector3(x * scale.X, (y + 1) * scale.Y, z * scale.Z),
                                           v000, v010);
                        var p1 = Interpolate(origin + new Vector3((x + 1) * scale.X, y * scale.Y, z * scale.Z),
                                           origin + new Vector3((x + 1) * scale.X, (y + 1) * scale.Y, z * scale.Z),
                                           v100, v110);
                        var p2 = Interpolate(origin + new Vector3(x * scale.X, y * scale.Y, (z + 1) * scale.Z),
                                           origin + new Vector3(x * scale.X, (y + 1) * scale.Y, (z + 1) * scale.Z),
                                           v001, v011);

                        vertices.Add(p0);
                        vertices.Add(p2);
                        vertices.Add(p1);
                    }

                    // Z-direction face
                    if ((v000 < 0) != (v001 < 0))
                    {
                        var p0 = Interpolate(origin + new Vector3(x * scale.X, y * scale.Y, z * scale.Z),
                                           origin + new Vector3(x * scale.X, y * scale.Y, (z + 1) * scale.Z),
                                           v000, v001);
                        var p1 = Interpolate(origin + new Vector3((x + 1) * scale.X, y * scale.Y, z * scale.Z),
                                           origin + new Vector3((x + 1) * scale.X, y * scale.Y, (z + 1) * scale.Z),
                                           v100, v101);
                        var p2 = Interpolate(origin + new Vector3(x * scale.X, (y + 1) * scale.Y, z * scale.Z),
                                           origin + new Vector3(x * scale.X, (y + 1) * scale.Y, (z + 1) * scale.Z),
                                           v010, v011);

                        vertices.Add(p0);
                        vertices.Add(p1);
                        vertices.Add(p2);
                    }
                }
            }
        }

        return vertices;
    }

    /// <summary>
    /// Linear interpolation between two points based on SDF values
    /// </summary>
    private static Vector3 Interpolate(Vector3 p0, Vector3 p1, double v0, double v1)
    {
        if (Math.Abs(v0 - v1) < 1e-6)
            return (p0 + p1) * 0.5;

        double t = -v0 / (v1 - v0);
        t = Math.Max(0.0, Math.Min(1.0, t));

        return p0 + (p1 - p0) * t;
    }
}
