using SDF;
using SDF.Examples;
using static SDF.Primitives;
using static SDF.Operations;
using static SDF.Constants;

Console.WriteLine("SDF C# Examples");
Console.WriteLine("===============");
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
