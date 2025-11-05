using SDF;
using static SDF.Primitives;
using static SDF.Constants;

// This is the C# port of the canonical CSG example from the Python library
// Original Python code:
// f = sphere(1) & box(1.5)
// c = cylinder(0.5)
// f -= c.orient(X) | c.orient(Y) | c.orient(Z)
// f.save('out.stl')

Console.WriteLine("SDF C# Library - Example");
Console.WriteLine("Generating CSG example mesh...");
Console.WriteLine();

// Create a sphere intersected with a box
var f = Sphere(1.0) & Box(1.5);

// Create a cylinder and subtract it in three orientations
var c = Cylinder(0.5);
f = f - (c.Orient(X) | c.Orient(Y) | c.Orient(Z));

// Save to STL file
f.Save("out.stl", verbose: true);

Console.WriteLine();
Console.WriteLine("Done! Mesh saved to out.stl");
