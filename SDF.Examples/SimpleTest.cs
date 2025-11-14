using SDF;
using static SDF.Primitives;
using static SDF.Constants;

namespace SDF.Examples;

public static class SimpleTest
{
    public static void Run()
    {
        Console.WriteLine("Running simple sphere test...");
        
        // Create a simple sphere
        var sphere = Sphere(1.0);
        
        // Save with lower resolution for faster testing
        sphere.Save("test-sphere.stl", samples: 1 << 18, verbose: true);
        
        Console.WriteLine("Sphere test complete!");
    }
}
