using SDF;
using static SDF.Primitives;
using static SDF.Operations;
using static SDF.Constants;
using System.Numerics;

namespace SDF.Examples;

/// <summary>
/// Example showing various transformations and operations
/// </summary>
public static class TwistedBoxExample
{
    public static void Generate()
    {
        Console.WriteLine("Generating twisted box example...");
        
        // Create a box and twist it
        var twistedBox = Box(new Vector3(2, 2, 4))
            .Twist(Pi / 2);
        
        twistedBox.Save("twisted_box.stl", samples: 1 << 20);
        Console.WriteLine("Generated twisted_box.stl");
    }
}
