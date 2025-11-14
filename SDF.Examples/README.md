# SDF C# Examples

This directory contains comprehensive examples demonstrating all features of the SDF C# library.

## Running the Examples

### Default Examples
Run the basic examples that demonstrate core functionality:

```bash
dotnet run --project SDF.Examples
```

This will generate:
- `test-sphere.stl` - Simple sphere primitive
- `csg-example.stl` - CSG operations (sphere & box with cylindrical holes)
- `transform-example.stl` - Transformations (torus on box)

### Comprehensive Examples
Run all examples demonstrating every feature of the library:

```bash
dotnet run --project SDF.Examples -- --comprehensive
```

This will generate 7 STL files showcasing:

1. **primitives.stl** - All primitive shapes:
   - Sphere
   - Box
   - Capped Cylinder
   - Torus
   - Capsule
   - Rounded Box
   - Ellipsoid

2. **boolean-ops.stl** - Boolean operations:
   - Union (|)
   - Intersection (&)
   - Difference (-)

3. **transformations.stl** - Transformation operations:
   - Translate
   - Rotate
   - Scale
   - Orient

4. **deformations.stl** - Deformation operations:
   - Twist
   - Bend
   - Elongate

5. **modifiers.stl** - Modifier operations:
   - Dilate (expand)
   - Erode (shrink)
   - Shell (hollow)

6. **complex-scene.stl** - Complex scene combining multiple features:
   - Twisted pillar
   - Orbiting spheres
   - Connecting tori
   - Platform base

7. **smooth-ops.stl** - Smooth boolean operations:
   - Smooth Union
   - Smooth Intersection
   - Smooth Difference

## Example Code

### Basic Example - CSG Operations

```csharp
using SDF;
using static SDF.Primitives;
using static SDF.Operations;
using static SDF.Constants;

// Create a sphere intersected with a box
var f = Sphere(1.0) & Box(1.5);

// Create cylinders and subtract them
var c = Cylinder(0.5);
f = f - (c.Orient(X) | c.Orient(Y) | c.Orient(Z));

// Save to STL file
f.Save("output.stl");
```

### Using Transformations

```csharp
// Create and transform a shape
var shape = Torus(1.0, 0.25)
    .Rotate(Pi / 4, X)
    .Translate(new Vector3(0, 0, 1.5));

shape.Save("transformed.stl");
```

### Deformations

```csharp
// Create a twisted box
var twisted = Box(new Vector3(1, 1, 3))
    .Twist(Pi / 2);

twisted.Save("twisted-box.stl");
```

### Smooth Operations

```csharp
// Create smooth union
var smoothUnion = Sphere(0.8).K(0.3) | Box(1.2).K(0.3);

smoothUnion.Save("smooth-union.stl");
```

## Available Features

### Primitives
- `Sphere(radius, center?)` - Sphere
- `Box(size, center?)` - Box (uniform or non-uniform)
- `Cylinder(radius)` - Infinite cylinder along Z
- `CappedCylinder(a, b, radius)` - Cylinder between two points
- `Capsule(a, b, radius)` - Capsule between two points
- `Torus(r1, r2)` - Torus
- `Plane(normal?, point?)` - Infinite plane
- `Slab(x0?, y0?, z0?, x1?, y1?, z1?)` - Bounded region
- `RoundedBox(size, radius)` - Box with rounded corners
- `Ellipsoid(size)` - Ellipsoid with different radii

### Boolean Operations
- `a | b` - Union
- `a & b` - Intersection
- `a - b` - Difference
- `.K(smoothing)` - Enable smooth blending

### Transformations
- `.Translate(offset)` - Move
- `.Scale(factor)` - Uniform scaling
- `.Rotate(angle, axis?)` - Rotate around axis
- `.Orient(direction)` - Orient Z-axis to direction

### Deformations
- `.Twist(k)` - Twist around Z-axis
- `.Bend(k)` - Bend
- `.Elongate(size)` - Elongate along axes

### Modifiers
- `.Dilate(r)` - Expand
- `.Erode(r)` - Shrink
- `.Shell(thickness)` - Create hollow shell

### Mesh Generation
- `.Generate(...)` - Generate triangle mesh
- `.Save(path, ...)` - Save to STL file

## Customization Options

When generating meshes, you can customize:

```csharp
shape.Save("output.stl",
    step: 0.01,           // Resolution (smaller = higher quality)
    samples: 1 << 23,     // Number of sample points
    batchSize: 32,        // Batch size for parallel processing
    sparse: true,         // Skip empty regions
    verbose: true         // Show progress
);
```

## Example Files

### SimpleTest.cs
Basic sphere generation demonstrating the simplest usage.

### CSGExample.cs
Classic CSG example matching the Python library's canonical example.

### TransformExample.cs
Demonstrates transformations (rotate, translate) with a torus on a box.

### TwistedBoxExample.cs
Shows the twist deformation creating a twisted box.

### AdvancedExample.cs
Contains various advanced examples (rounded boxes, shells, etc.).

### ComprehensiveExamples.cs
**NEW!** Complete showcase of all library features organized into 7 categories.

## Performance Tips

1. Use `verbose: false` for batch processing
2. Adjust `samples` based on desired quality (higher = better but slower)
3. Use `sparse: true` to skip empty regions
4. Smaller `step` values create higher resolution meshes but take longer

## Output Files

All generated STL files are in binary format and can be:
- Imported into 3D modeling software (Blender, Mesh mixer, etc.)
- Used for 3D printing
- Visualized in STL viewers
- Further processed with mesh tools

## Next Steps

1. Modify the examples to create your own shapes
2. Combine multiple operations to create complex models
3. Experiment with parameters to understand their effects
4. Create custom SDFs by combining primitives and operations

## Additional Resources

- See `README-CSharp.md` in the root directory for full API documentation
- Check `CSHARP_PORT_SUMMARY.md` for implementation details
- Original Python library: https://github.com/fogleman/sdf
