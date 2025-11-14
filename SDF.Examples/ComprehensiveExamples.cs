using SDF;
using static SDF.Primitives;
using static SDF.Operations;
using static SDF.Constants;

namespace SDF.Examples;

/// <summary>
/// Comprehensive examples demonstrating all features of the SDF C# library
/// </summary>
public static class ComprehensiveExamples
{
    /// <summary>
    /// Run all comprehensive examples
    /// </summary>
    public static void RunAll()
    {
        Console.WriteLine("Running Comprehensive SDF Examples");
        Console.WriteLine("===================================");
        Console.WriteLine();

        // Example 1: All Primitives
        Console.WriteLine("Example 1: All Primitive Shapes");
        AllPrimitives();
        Console.WriteLine();

        // Example 2: Boolean Operations
        Console.WriteLine("Example 2: Boolean Operations (Union, Intersection, Difference)");
        BooleanOperations();
        Console.WriteLine();

        // Example 3: Transformations
        Console.WriteLine("Example 3: Transformations (Translate, Rotate, Scale, Orient)");
        TransformationsDemo();
        Console.WriteLine();

        // Example 4: Deformations
        Console.WriteLine("Example 4: Deformations (Twist, Bend, Elongate)");
        DeformationsDemo();
        Console.WriteLine();

        // Example 5: Modifiers
        Console.WriteLine("Example 5: Modifiers (Dilate, Erode, Shell)");
        ModifiersDemo();
        Console.WriteLine();

        // Example 6: Complex Scene
        Console.WriteLine("Example 6: Complex Scene Combining Multiple Features");
        ComplexScene();
        Console.WriteLine();

        // Example 7: Smooth Operations
        Console.WriteLine("Example 7: Smooth Boolean Operations");
        SmoothOperations();
        Console.WriteLine();

        Console.WriteLine("All comprehensive examples complete!");
        Console.WriteLine();
        Console.WriteLine("Generated Files:");
        Console.WriteLine("  - primitives.stl");
        Console.WriteLine("  - boolean-ops.stl");
        Console.WriteLine("  - transformations.stl");
        Console.WriteLine("  - deformations.stl");
        Console.WriteLine("  - modifiers.stl");
        Console.WriteLine("  - complex-scene.stl");
        Console.WriteLine("  - smooth-ops.stl");
    }

    /// <summary>
    /// Example 1: Demonstrate all primitive shapes
    /// </summary>
    public static void AllPrimitives()
    {
        // Create a grid of different primitives
        var spacing = 4.0;

        var sphere = Sphere(0.8).Translate(new Vector3(0, 0, 0));
        var box = Box(1.5).Translate(new Vector3(spacing, 0, 0));
        var cylinder = CappedCylinder(
            new Vector3(spacing * 2, 0, -0.75),
            new Vector3(spacing * 2, 0, 0.75),
            0.6
        );
        var torus = Torus(0.8, 0.3).Translate(new Vector3(spacing * 3, 0, 0));
        var capsule = Capsule(
            new Vector3(0, spacing, -0.75),
            new Vector3(0, spacing, 0.75),
            0.4
        );
        var roundedBox = RoundedBox(new Vector3(1.5, 1.5, 1.5), 0.2)
            .Translate(new Vector3(spacing, spacing, 0));
        var ellipsoid = Ellipsoid(new Vector3(1.0, 0.7, 0.5))
            .Translate(new Vector3(spacing * 2, spacing, 0));

        // Combine all primitives
        var allPrimitives = sphere | box | cylinder | torus | capsule | roundedBox | ellipsoid;

        Console.WriteLine("  Generating primitives showcase...");
        allPrimitives.Save("primitives.stl", samples: 1 << 23, verbose: false);
        Console.WriteLine("  ✓ Saved to primitives.stl");
    }

    /// <summary>
    /// Example 2: Boolean operations
    /// </summary>
    public static void BooleanOperations()
    {
        var spacing = 3.5;

        // Union (|)
        var union = (Sphere(0.8) | Box(1.2)).Translate(new Vector3(0, 0, 0));

        // Intersection (&)
        var intersection = (Sphere(0.8) & Box(1.2)).Translate(new Vector3(spacing, 0, 0));

        // Difference (-)
        var difference = (Sphere(0.8) - Box(1.0)).Translate(new Vector3(spacing * 2, 0, 0));

        var combined = union | intersection | difference;

        Console.WriteLine("  Generating boolean operations demo...");
        combined.Save("boolean-ops.stl", samples: 1 << 22, verbose: false);
        Console.WriteLine("  ✓ Saved to boolean-ops.stl");
    }

    /// <summary>
    /// Example 3: Transformations
    /// </summary>
    public static void TransformationsDemo()
    {
        var spacing = 3.0;

        // Translate
        var translated = Box(1.0).Translate(new Vector3(0, 0, 1));

        // Rotate
        var rotated = Box(new Vector3(2, 0.5, 0.5))
            .Rotate(Pi / 4, Z)
            .Translate(new Vector3(spacing, 0, 0));

        // Scale
        var scaled = Sphere(0.5).Scale(1.5).Translate(new Vector3(spacing * 2, 0, 0));

        // Orient
        var oriented = CappedCylinder(
            new Vector3(0, 0, -0.5),
            new Vector3(0, 0, 0.5),
            0.3
        ).Orient(new Vector3(1, 1, 0).Normalize())
        .Translate(new Vector3(spacing * 3, 0, 0));

        var combined = translated | rotated | scaled | oriented;

        Console.WriteLine("  Generating transformations demo...");
        combined.Save("transformations.stl", samples: 1 << 22, verbose: false);
        Console.WriteLine("  ✓ Saved to transformations.stl");
    }

    /// <summary>
    /// Example 4: Deformations
    /// </summary>
    public static void DeformationsDemo()
    {
        var spacing = 3.5;

        // Twist
        var twisted = Box(new Vector3(1, 1, 3))
            .Twist(Pi / 2)
            .Translate(new Vector3(0, 0, 0));

        // Bend
        var bent = Box(new Vector3(2.5, 0.8, 0.8))
            .Bend(0.3)
            .Translate(new Vector3(spacing, 0, 0));

        // Elongate
        var elongated = Sphere(0.6)
            .Elongate(new Vector3(1.0, 0.3, 0.3))
            .Translate(new Vector3(spacing * 2, 0, 0));

        var combined = twisted | bent | elongated;

        Console.WriteLine("  Generating deformations demo...");
        combined.Save("deformations.stl", samples: 1 << 22, verbose: false);
        Console.WriteLine("  ✓ Saved to deformations.stl");
    }

    /// <summary>
    /// Example 5: Modifiers
    /// </summary>
    public static void ModifiersDemo()
    {
        var spacing = 3.0;

        // Dilate (expand)
        var dilated = Sphere(0.7).Dilate(0.2).Translate(new Vector3(0, 0, 0));

        // Erode (shrink)
        var eroded = Box(1.5).Erode(0.2).Translate(new Vector3(spacing, 0, 0));

        // Shell
        var shell = Sphere(1.0).Shell(0.1).Translate(new Vector3(spacing * 2, 0, 0));

        var combined = dilated | eroded | shell;

        Console.WriteLine("  Generating modifiers demo...");
        combined.Save("modifiers.stl", samples: 1 << 22, verbose: false);
        Console.WriteLine("  ✓ Saved to modifiers.stl");
    }

    /// <summary>
    /// Example 6: Complex scene combining multiple features
    /// </summary>
    public static void ComplexScene()
    {
        // Base platform
        var platform = Box(new Vector3(6, 6, 0.5)).Translate(new Vector3(0, 0, -1));

        // Central twisted pillar
        var pillar = Box(new Vector3(0.8, 0.8, 4))
            .Twist(Pi)
            .Translate(new Vector3(0, 0, 1));

        // Orbiting spheres
        var sphere1 = Sphere(0.5).Translate(new Vector3(2, 0, 1));
        var sphere2 = Sphere(0.5).Translate(new Vector3(-2, 0, 1));
        var sphere3 = Sphere(0.5).Translate(new Vector3(0, 2, 1));
        var sphere4 = Sphere(0.5).Translate(new Vector3(0, -2, 1));

        // Connecting tori
        var torus1 = Torus(1.8, 0.15)
            .Rotate(Pi / 2, X)
            .Translate(new Vector3(0, 0, 1));

        var torus2 = Torus(1.8, 0.15)
            .Rotate(Pi / 2, Y)
            .Translate(new Vector3(0, 0, 1));

        // Combine all elements
        var scene = platform | pillar | sphere1 | sphere2 | sphere3 | sphere4 | torus1 | torus2;

        Console.WriteLine("  Generating complex scene...");
        scene.Save("complex-scene.stl", samples: 1 << 23, verbose: false);
        Console.WriteLine("  ✓ Saved to complex-scene.stl");
    }

    /// <summary>
    /// Example 7: Smooth boolean operations
    /// </summary>
    public static void SmoothOperations()
    {
        var spacing = 3.5;

        // Smooth union
        var smoothUnion = Sphere(0.8).K(0.3) | Box(1.2).K(0.3);
        smoothUnion = smoothUnion.Translate(new Vector3(0, 0, 0));

        // Smooth intersection
        var smoothIntersection = Sphere(0.8).K(0.3) & Box(1.2).K(0.3);
        smoothIntersection = smoothIntersection.Translate(new Vector3(spacing, 0, 0));

        // Smooth difference
        var smoothDifference = Sphere(0.8).K(0.3) - Box(1.0).K(0.3);
        smoothDifference = smoothDifference.Translate(new Vector3(spacing * 2, 0, 0));

        var combined = smoothUnion | smoothIntersection | smoothDifference;

        Console.WriteLine("  Generating smooth operations demo...");
        combined.Save("smooth-ops.stl", samples: 1 << 22, verbose: false);
        Console.WriteLine("  ✓ Saved to smooth-ops.stl");
    }
}
