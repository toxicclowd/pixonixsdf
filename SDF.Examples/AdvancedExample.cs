using SDF;
using static SDF.Primitives;
using static SDF.Operations;
using static SDF.Constants;
using System.Numerics;

namespace SDF.Examples;

/// <summary>
/// More advanced examples showing the library capabilities
/// </summary>
public static class AdvancedExample
{
    public static void GenerateRoundedBox()
    {
        Console.WriteLine("Generating rounded box example...");
        
        var roundedBox = RoundedBox(new Vector3(2, 2, 2), 0.25);
        roundedBox.Save("rounded_box.stl", samples: 1 << 20);
        
        Console.WriteLine("Generated rounded_box.stl");
    }

    public static void GenerateCapsule()
    {
        Console.WriteLine("Generating capsule example...");
        
        var capsule = Capsule(-Z * 2, Z * 2, 0.5);
        capsule.Save("capsule.stl", samples: 1 << 20);
        
        Console.WriteLine("Generated capsule.stl");
    }

    public static void GenerateTorusBox()
    {
        Console.WriteLine("Generating torus-box intersection...");
        
        var shape = Torus(1.5, 0.5) & Box(new Vector3(3, 3, 0.5f));
        shape.Save("torus_box.stl", samples: 1 << 20);
        
        Console.WriteLine("Generated torus_box.stl");
    }

    public static void GenerateRepeatedSphere()
    {
        Console.WriteLine("Generating repeated sphere example...");
        
        // Create a sphere and repeat it in a pattern
        var sphere = Sphere(0.5);
        var repeated = sphere.Repeat(new Vector3(2, 2, 2), new Vector3(2, 2, 2));
        
        repeated.Save("repeated_sphere.stl", samples: 1 << 20);
        Console.WriteLine("Generated repeated_sphere.stl");
    }

    public static void GenerateShell()
    {
        Console.WriteLine("Generating shell example...");
        
        // Create a hollow sphere
        var hollowSphere = Sphere(1.0).Shell(0.1) & Plane(-Z);
        hollowSphere.Save("shell.stl", samples: 1 << 20);
        
        Console.WriteLine("Generated shell.stl");
    }
}
