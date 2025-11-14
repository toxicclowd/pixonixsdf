using SDF;
using static SDF.Primitives;
using static SDF.Constants;

namespace SDF.Examples;

/// <summary>
/// Demonstrates transformation operations
/// </summary>
public static class TransformExample
{
    public static void Run()
    {
        Console.WriteLine("Running transformation example...");
        Console.WriteLine();
        
        // Create a torus
        var torus = Torus(1.0, 0.25);
        
        // Rotate it 45 degrees around X axis
        torus = torus.Rotate(Pi / 4, X);
        
        // Translate it up
        torus = torus.Translate(new Vector3(0, 0, 1.5));
        
        // Create a box
        var baseBox = Box(new Vector3(3, 3, 0.5));
        
        // Combine them with union
        var result = baseBox | torus;
        
        // Save to STL file
        result.Save("transform-example.stl", samples: 1 << 20, verbose: true);
        
        Console.WriteLine();
        Console.WriteLine("Transform example complete! Mesh saved to transform-example.stl");
    }
}
