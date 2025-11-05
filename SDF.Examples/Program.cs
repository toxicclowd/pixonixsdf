using SDF;
using static SDF.Primitives;
using static SDF.Operations;
using static SDF.Constants;

// This is the C# equivalent of the Python example:
// f = sphere(1) & box(1.5)
// c = cylinder(0.5)
// f -= c.orient(X) | c.orient(Y) | c.orient(Z)
// f.save('out.stl')

Console.WriteLine("Generating SDF mesh example...");
Console.WriteLine();

var f = Sphere(1.0) & Box(1.5);

var c = Cylinder(0.5);
f = f - (c.Orient(X) | c.Orient(Y) | c.Orient(Z));

f.Save("out.stl");

Console.WriteLine();
Console.WriteLine("Done! Generated out.stl");
