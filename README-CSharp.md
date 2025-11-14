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
# SDF - C# Port

This is a C# port of the Python SDF library for generating 3D meshes based on Signed Distance Functions (SDFs).

## Overview

Generate 3D meshes using a simple and expressive C# API based on signed distance functions. This library uses the marching cubes algorithm to convert SDFs into triangle meshes that can be exported to STL files for 3D printing or use in 3D applications.

## Features

- **Simple, expressive API** - Define complex 3D shapes with minimal code
- **Boolean operations** - Combine shapes with union, intersection, and difference
- **Transformations** - Translate, rotate, scale, and orient shapes
- **Parallel processing** - Utilizes multiple CPU cores for fast mesh generation
- **STL export** - Save meshes as binary STL files

## Requirements

- .NET 9.0 or later
- NuGet packages (automatically installed):
  - MathNet.Numerics
  - System.Numerics.Vectors

## Installation

1. Clone the repository:
```bash
git clone https://github.com/toxicclowd/pixonixsdf.git
cd pixonixsdf
```

2. Build the solution:
```bash
dotnet build
```

3. Run the example:
```bash
dotnet run --project SDF.Examples
```

## Example Usage

Here's a complete example that generates a classic CSG (Constructive Solid Geometry) model:

```csharp
using SDF;
using static SDF.Primitives;
using static SDF.Operations;
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
// Create a cylinder and orient it in three directions
var c = Cylinder(0.5);
f = f - (c.Orient(X) | c.Orient(Y) | c.Orient(Z));

// Save the result as an STL file
f.Save("out.stl");
```

This generates a sphere-box intersection with cylindrical holes along each axis.

## API Reference

### Primitives

#### Sphere
```csharp
var s = Sphere(radius: 1.0, center: Constants.Origin);
```

#### Box
```csharp
var b = Box(size: 1.5);  // Uniform size
var b = Box(new Vector3(1, 2, 3));  // Different dimensions
```

#### Cylinder
```csharp
var c = Cylinder(radius: 0.5);  // Infinite cylinder along Z axis
```

#### Plane
```csharp
var p = Plane(normal: Constants.Up, point: Constants.Origin);
```

#### Torus
```csharp
var t = Torus(r1: 1.0, r2: 0.25);
```

### Boolean Operations

Use operator overloading for intuitive boolean operations:

```csharp
// Union (|)
var union = shape1 | shape2;

// Intersection (&)
var intersection = shape1 & shape2;

// Difference (-)
var difference = shape1 - shape2;
```

For smooth blending, use the `.K()` method:

```csharp
var smoothUnion = shape1 | shape2.K(0.25);
var smoothDifference = shape1 - shape2.K(0.25);
var smoothIntersection = shape1 & shape2.K(0.25);
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
#### Translate
```csharp
var translated = shape.Translate(new Vector3(1, 2, 3));
```

#### Scale
```csharp
var scaled = shape.Scale(2.0);  // Uniform scaling
```

#### Rotate
```csharp
var rotated = shape.Rotate(Math.PI / 4, Constants.X);  // Rotate 45° around X axis
```

#### Orient
```csharp
var oriented = shape.Orient(Constants.Y);  // Point shape towards Y axis
```

### Saving Meshes

Save a shape as an STL file:

```csharp
shape.Save("output.stl");

// With custom parameters
shape.Save("output.stl", 
    step: 0.01,              // Resolution
    samples: 1 << 24,        // Number of sample points
    workers: 8,              // Number of worker threads
    verbose: true            // Print progress
);
```

### Mesh Generation

Generate triangle vertices without saving to file:

```csharp
List<Vector3> triangles = shape.Generate(
    step: 0.01,
    samples: 1 << 22,
    workers: Environment.ProcessorCount
);

Console.WriteLine($"Generated {triangles.Count / 3} triangles");
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
- **SDF.CSharp/** - Core library
  - `Constants.cs` - Mathematical constants and utility functions
  - `SDF3.cs` - Main SDF class with operator overloading
  - `Primitives.cs` - Basic 3D primitive shapes
  - `Operations.cs` - Transformations and boolean operations
  - `MeshGenerator.cs` - Core mesh generation engine
  - `MarchingCubes.cs` - Marching cubes algorithm
  - `StlWriter.cs` - Binary STL file writer
  - `SDF3Extensions.cs` - Extension methods for fluent API

- **SDF.Examples/** - Example programs
  - `Program.cs` - Basic CSG example

## Differences from Python Version

1. **Type System**: C# uses strong typing with `Vector3` and explicit types
2. **Operator Overloading**: Boolean operations use `|`, `&`, `-` just like Python
3. **Namespaces**: Organized into proper C# namespaces
4. **Performance**: Comparable or better performance due to .NET's JIT compilation
5. **API Style**: Uses C# conventions (PascalCase, etc.)

## Current Limitations

This is a functional port with the core features implemented. Some features from the Python version are not yet ported:

- 2D SDFs and operations
- Text and image extrusion
- Some advanced primitives (rounded boxes, capsules, etc.)
- Mesh loading from files
- Easing functions
- Some advanced deformation operations

These features can be added in future updates as needed.

## Building from Source

```bash
# Build the solution
dotnet build SDF.sln

# Run tests (when available)
dotnet test

# Create a NuGet package
dotnet pack SDF.CSharp/SDF.csproj
```

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests to improve the C# port.

## License

This project follows the same MIT license as the original Python SDF library.

## Acknowledgments

- Original Python SDF library by Michael Fogleman
- [Inigo Quilez](https://iquilezles.org/) for excellent SDF documentation
- Marching cubes algorithm researchers

## See Also

- [Original Python SDF Repository](https://github.com/fogleman/sdf)
- [3D Signed Distance Functions](https://iquilezles.org/www/articles/distfunctions/distfunctions.htm)
- [Constructive Solid Geometry](https://en.wikipedia.org/wiki/Constructive_solid_geometry)
