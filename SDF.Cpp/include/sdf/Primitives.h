#pragma once

#include "SDF3.h"
#include "Vector3.h"
#include "Constants.h"

namespace sdf {

/**
 * @brief 3D primitive shapes
 */

// Basic primitives
SDF3 sphere(double radius, const Vector3& center);
SDF3 sphere(double radius);
SDF3 sphere();
SDF3 box(const Vector3& size, const Vector3& center);
SDF3 box(const Vector3& size);
SDF3 box(double size, const Vector3& center);
SDF3 box(double size);
SDF3 roundedBox(const Vector3& size, double radius);
SDF3 torus(double r1, double r2);
SDF3 capsule(const Vector3& a, const Vector3& b, double radius);
SDF3 cappedCylinder(const Vector3& a, const Vector3& b, double radius);
SDF3 cylinder(double radius);
SDF3 ellipsoid(const Vector3& size);

// Infinite primitives
SDF3 plane(const Vector3& normal, const Vector3& point);
SDF3 plane();
SDF3 slab(
    double x0 = -1e9, double x1 = 1e9,
    double y0 = -1e9, double y1 = 1e9,
    double z0 = -1e9, double z1 = 1e9
);

// Additional primitives
SDF3 cone(double angle, double height = 1e9);
SDF3 roundedCone(double r1, double r2, double h);
SDF3 cappedCone(const Vector3& a, const Vector3& b, double ra, double rb);

} // namespace sdf
