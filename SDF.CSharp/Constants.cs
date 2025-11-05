using System;
using System.Numerics;

namespace SDF;

/// <summary>
/// Constants and common values for SDF operations
/// </summary>
public static class Constants
{
    public const double Pi = Math.PI;
    
    public static readonly Vector3 Origin = Vector3.Zero;
    public static readonly Vector3 X = Vector3.UnitX;
    public static readonly Vector3 Y = Vector3.UnitY;
    public static readonly Vector3 Z = Vector3.UnitZ;
    public static readonly Vector3 Up = Z;
    
    public static double Degrees(double radians) => radians * 180.0 / Pi;
    public static double Radians(double degrees) => degrees * Pi / 180.0;
}
