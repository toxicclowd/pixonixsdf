using SDF.Examples;

Console.WriteLine("SDF C# Library - Examples");
Console.WriteLine("==========================");
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
﻿using SDF;
using SDF.Examples;
using static SDF.Primitives;
using static SDF.Operations;
using static SDF.Constants;

Console.WriteLine("SDF C# Examples");
Console.WriteLine("===============");
Console.WriteLine();

// Main example - This is the C# equivalent of the Python example:
// f = sphere(1) & box(1.5)
// c = cylinder(0.5)
// f -= c.orient(X) | c.orient(Y) | c.orient(Z)
// f.save('out.stl')

Console.WriteLine("Generating main CSG example...");
Console.WriteLine();

var f = Sphere(1.0) & Box(1.5);

var c = Cylinder(0.5);
f = f - (c.Orient(X) | c.Orient(Y) | c.Orient(Z));

f.Save("out.stl");

Console.WriteLine();
Console.WriteLine("Main example complete! Generated out.stl");
Console.WriteLine();

// Optional: Generate additional examples
// Uncomment to generate more examples:

// TwistedBoxExample.Generate();
// AdvancedExample.GenerateRoundedBox();
// AdvancedExample.GenerateCapsule();
// AdvancedExample.GenerateTorusBox();
// AdvancedExample.GenerateRepeatedSphere();
// AdvancedExample.GenerateShell();

Console.WriteLine("All examples complete!");
