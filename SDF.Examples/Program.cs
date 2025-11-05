using SDF;
using SDF.Examples;
using static SDF.Primitives;
using static SDF.Constants;

Console.WriteLine("SDF C# Library - Examples");
Console.WriteLine("==========================");
Console.WriteLine();

// Run simple test first
Console.WriteLine("1. Simple Sphere Test");
SimpleTest.Run();
Console.WriteLine();

// Uncomment to run the full CSG example
// Console.WriteLine("2. CSG Example (sphere & box with cylinder holes)");
// This is the C# port of the canonical CSG example from the Python library
// Original Python code:
// f = sphere(1) & box(1.5)
// c = cylinder(0.5)
// f -= c.orient(X) | c.orient(Y) | c.orient(Z)
// f.save('out.stl')

// var f = Sphere(1.0) & Box(1.5);
// var c = Cylinder(0.5);
// f = f - (c.Orient(X) | c.Orient(Y) | c.Orient(Z));
// f.Save("out.stl", verbose: true);

Console.WriteLine("All tests complete!");
