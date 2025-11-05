using SDF;
using static SDF.Primitives;
using static SDF.Constants;

namespace SDF.Examples;

/// <summary>
/// The canonical CSG example - matching the Python version
/// Original Python code:
///   f = sphere(1) & box(1.5)
///   c = cylinder(0.5)
///   f -= c.orient(X) | c.orient(Y) | c.orient(Z)
///   f.save('out.stl')
/// </summary>
public static class CSGExample
{
    public static void Run()
    {
        Console.WriteLine("Running CSG example (sphere & box with cylinder holes)...");
        Console.WriteLine();
        
        // Create a sphere intersected with a box
        var f = Sphere(1.0) & Box(1.5);
        
        // Create a cylinder and subtract it in three orientations
        var c = Cylinder(0.5);
        f = f - (c.Orient(X) | c.Orient(Y) | c.Orient(Z));
        
        // Save to STL file with moderate resolution
        f.Save("csg-example.stl", samples: 1 << 20, verbose: true);
        
        Console.WriteLine();
        Console.WriteLine("CSG example complete! Mesh saved to csg-example.stl");
    }
}
