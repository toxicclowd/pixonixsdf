# SDF C# Port

This is a C# port of the Python `sdf` library for generating 3D meshes from Signed Distance Functions (SDFs).

## Overview

The SDF library allows you to generate 3D meshes using simple mathematical functions and boolean operations. This C# port maintains the same API philosophy as the Python version while leveraging .NET's performance and type safety.

## Requirements

- .NET 9.0 SDK or later

## Building

```bash
dotnet build
```

## Running Examples

```bash
cd SDF.Examples
dotnet run
```

## Quick Start

Here's a simple example that creates a sphere:

```csharp
using SDF;
using static SDF.Primitives;

// Create a sphere with radius 1.0
var sphere = Sphere(1.0);

// Save to STL file
sphere.Save("sphere.stl");
```

## CSG Example

The canonical Constructive Solid Geometry example (matching the Python version):

```csharp
using SDF;
using static SDF.Primitives;
using static SDF.Constants;

// Create a sphere intersected with a box
var f = Sphere(1.0) & Box(1.5);

// Create a cylinder and subtract it in three orientations
var c = Cylinder(0.5);
f = f - (c.Orient(X) | c.Orient(Y) | c.Orient(Z));

// Save to STL file
f.Save("out.stl");
```

## Supported Features

### Primitives

- **Sphere**: `Sphere(radius, center)`
- **Box**: `Box(size, center)` or `Box(sizeVector, center)`
- **Cylinder**: `Cylinder(radius)` (infinite along Z-axis)
- **CappedCylinder**: `CappedCylinder(a, b, radius)`
- **Torus**: `Torus(r1, r2)`
- **Plane**: `Plane(normal, point)`

### Boolean Operations

- **Union**: `a | b`
- **Intersection**: `a & b`
- **Difference**: `a - b`
- **Smooth operations**: Use `.K(smoothing)` to add smoothing

Example:
```csharp
var smoothUnion = sphere.K(0.25) | box;
```

### Transformations

- **Translate**: `sdf.Translate(offset)`
- **Scale**: `sdf.Scale(factor)`
- **Rotate**: `sdf.Rotate(angle, axis)`
- **Orient**: `sdf.Orient(direction)` - Rotates Z-axis to point in specified direction

### Mesh Generation Options

```csharp
sdf.Save("output.stl", 
    step: 0.01,                           // Step size (resolution)
    bounds: (minVector, maxVector),       // Optional custom bounds
    samples: 1 << 22,                     // Number of sample points
    batchSize: 32,                        // Batch size for processing
    sparse: true,                         // Enable sparse sampling
    verbose: true);                       // Show progress
```

## Project Structure

- **SDF.CSharp**: Core library containing:
  - `Vector3.cs`: 3D vector mathematics
  - `Constants.cs`: Mathematical constants and direction vectors
  - `SDF3.cs`: Main SDF class
  - `Primitives.cs`: 3D primitive shapes
  - `Operations.cs`: Boolean operations and transformations
  - `Core.cs`: Mesh generation engine
  - `MarchingCubes.cs`: Surface extraction algorithm
  - `StlWriter.cs`: Binary STL file writer

- **SDF.Examples**: Example programs demonstrating library usage

## Differences from Python Version

1. **Type Safety**: C# version uses strong typing throughout
2. **Performance**: Leverages .NET parallel processing for batch operations
3. **API Style**: Uses C# conventions (PascalCase) while maintaining similar structure
4. **Operator Overloading**: Boolean operations use `|`, `&`, `-` operators like Python
5. **Extension Methods**: Transformations are implemented as extension methods

## Implementation Notes

- The mesh generation uses a simplified surface extraction algorithm based on marching cubes
- Parallel processing is used for batch operations to improve performance
- Bounds are automatically estimated if not provided
- The library outputs binary STL files for maximum compatibility

## Future Improvements

- Add more primitive shapes (pyramids, platonic solids, etc.)
- Implement 2D SDF support with extrusion
- Add text and image support
- Implement more transformation operations (twist, bend, etc.)
- Add support for other mesh output formats
- Optimize marching cubes implementation for better mesh quality

## License

Same license as the original Python library (MIT).

## Credits

Original Python library by Michael Fogleman: https://github.com/fogleman/sdf

C# port maintains the same design philosophy and API structure.
