using SDF;
using SDF.Examples;
using static SDF.Primitives;
using static SDF.Operations;
using static SDF.Constants;

Console.WriteLine("SDF C# Examples");
Console.WriteLine("===============");
Console.WriteLine();

// Determine which examples to run
var runComprehensive = args.Length > 0 && args[0] == "--comprehensive";

if (runComprehensive)
{
    // Run comprehensive examples demonstrating all features
    ComprehensiveExamples.RunAll();
}
else
{
    // Run default examples
    Console.WriteLine("Running Default Examples (use --comprehensive for all features)");
    Console.WriteLine();

    // Run simple test first
    Console.WriteLine("1. Simple Sphere Test");
    SimpleTest.Run();
    Console.WriteLine();

    // Run the full CSG example
    Console.WriteLine("2. CSG Example (sphere & box with cylinder holes)");
    CSGExample.Run();
    Console.WriteLine();

    // Run transformation example
    Console.WriteLine("3. Transformation Example (torus on box)");
    TransformExample.Run();
    Console.WriteLine();

    Console.WriteLine("All examples complete!");
    Console.WriteLine();
    Console.WriteLine("Generated files:");
    Console.WriteLine("  - test-sphere.stl");
    Console.WriteLine("  - csg-example.stl");
    Console.WriteLine("  - transform-example.stl");
    Console.WriteLine();
    Console.WriteLine("Tip: Run with --comprehensive to see all library features!");
}

