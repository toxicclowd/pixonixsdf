#include "sdf/Primitives.h"
#include <cmath>
#include <algorithm>

namespace sdf {

// Helper for clamp (C++17 compatible)
template<typename T>
static T clamp(T value, T min, T max) {
    return std::max(min, std::min(value, max));
}

// Helper functions
static double dot(const Vector3& a, const Vector3& b) {
    return a.dot(b);
}

static Vector3 abs(const Vector3& v) {
    return Vector3::abs(v);
}

static Vector3 max(const Vector3& v, double d) {
    return Vector3(std::max(v.x, d), std::max(v.y, d), std::max(v.z, d));
}

// Sphere
SDF3 sphere(double radius, const Vector3& center) {
    return SDF3([radius, center](const Vector3& p) {
        return (p - center).length() - radius;
    });
}

SDF3 sphere(double radius) {
    return sphere(radius, ORIGIN);
}

SDF3 sphere() {
    return sphere(1.0, ORIGIN);
}

// Box
SDF3 box(const Vector3& size, const Vector3& center) {
    Vector3 halfSize = size * 0.5;
    return SDF3([halfSize, center](const Vector3& p) {
        Vector3 q = abs(p - center) - halfSize;
        return max(q, 0.0).length() + std::min(std::max(q.x, std::max(q.y, q.z)), 0.0);
    });
}

SDF3 box(const Vector3& size) {
    return box(size, ORIGIN);
}

SDF3 box(double size, const Vector3& center) {
    return box(Vector3(size, size, size), center);
}

SDF3 box(double size) {
    return box(Vector3(size, size, size), ORIGIN);
}

// Rounded Box
SDF3 roundedBox(const Vector3& size, double radius) {
    Vector3 halfSize = size * 0.5 - Vector3(radius, radius, radius);
    return SDF3([halfSize, radius](const Vector3& p) {
        Vector3 q = abs(p) - halfSize;
        return max(q, 0.0).length() + std::min(std::max(q.x, std::max(q.y, q.z)), 0.0) - radius;
    });
}

// Torus
SDF3 torus(double r1, double r2) {
    return SDF3([r1, r2](const Vector3& p) {
        double qx = std::sqrt(p.x * p.x + p.y * p.y) - r1;
        return std::sqrt(qx * qx + p.z * p.z) - r2;
    });
}

// Capsule
SDF3 capsule(const Vector3& a, const Vector3& b, double radius) {
    return SDF3([a, b, radius](const Vector3& p) {
        Vector3 pa = p - a;
        Vector3 ba = b - a;
        double h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
        return (pa - ba * h).length() - radius;
    });
}

// Capped Cylinder
SDF3 cappedCylinder(const Vector3& a, const Vector3& b, double radius) {
    return SDF3([a, b, radius](const Vector3& p) {
        Vector3 ba = b - a;
        Vector3 pa = p - a;
        double baba = dot(ba, ba);
        double paba = dot(pa, ba);
        double x = (pa * baba - ba * paba).length() - radius * baba;
        double y = std::abs(paba - baba * 0.5) - baba * 0.5;
        double x2 = x * x;
        double y2 = y * y * baba;
        double d = (std::max(x, y) < 0.0) ? -std::min(x2, y2) : 
                   (((x > 0.0) ? x2 : 0.0) + ((y > 0.0) ? y2 : 0.0));
        return std::copysign(std::sqrt(std::abs(d)) / baba, d);
    });
}

// Cylinder (infinite)
SDF3 cylinder(double radius) {
    return SDF3([radius](const Vector3& p) {
        return std::sqrt(p.x * p.x + p.y * p.y) - radius;
    });
}

// Ellipsoid
SDF3 ellipsoid(const Vector3& size) {
    return SDF3([size](const Vector3& p) {
        double k0 = (p / size).length();
        double k1 = (p / (size * size)).length();
        return k0 * (k0 - 1.0) / k1;
    });
}

// Plane
SDF3 plane(const Vector3& normal, const Vector3& point) {
    Vector3 n = normal.normalized();
    return SDF3([n, point](const Vector3& p) {
        return dot(p - point, n);
    });
}

SDF3 plane() {
    return plane(UP, ORIGIN);
}

// Slab
SDF3 slab(double x0, double x1, double y0, double y1, double z0, double z1) {
    return SDF3([x0, x1, y0, y1, z0, z1](const Vector3& p) {
        double dx = std::max(std::max(x0 - p.x, p.x - x1), 0.0);
        double dy = std::max(std::max(y0 - p.y, p.y - y1), 0.0);
        double dz = std::max(std::max(z0 - p.z, p.z - z1), 0.0);
        
        double exterior = std::sqrt(dx * dx + dy * dy + dz * dz);
        
        double ix = (p.x >= x0 && p.x <= x1) ? 0.0 : std::min(std::abs(x0 - p.x), std::abs(x1 - p.x));
        double iy = (p.y >= y0 && p.y <= y1) ? 0.0 : std::min(std::abs(y0 - p.y), std::abs(y1 - p.y));
        double iz = (p.z >= z0 && p.z <= z1) ? 0.0 : std::min(std::abs(z0 - p.z), std::abs(z1 - p.z));
        
        double interior = -std::min(std::min(ix, iy), iz);
        
        return (exterior > 0.0) ? exterior : interior;
    });
}

// Cone
SDF3 cone(double angle, double height) {
    double c = std::cos(angle);
    double s = std::sin(angle);
    Vector3 q(s, c, 0);
    
    return SDF3([q, height](const Vector3& p) {
        double qDotP = q.x * std::sqrt(p.x * p.x + p.y * p.y) + q.y * p.z;
        double lxy = std::sqrt(p.x * p.x + p.y * p.y);
        Vector3 w(lxy, p.z, 0);
        Vector3 a = w - q * clamp(qDotP, 0.0, height);
        Vector3 b = w - q * Vector3(clamp(lxy, 0.0, height), 0, 0);
        double k = (qDotP < 0.0) ? -1.0 : 1.0;
        double d = std::min(dot(a, a), dot(b, b));
        double s = std::max(k * (w.x * q.y - w.y * q.x), k * (w.y - height));
        return std::sqrt(d) * std::copysign(1.0, s);
    });
}

// Rounded Cone
SDF3 roundedCone(double r1, double r2, double h) {
    return SDF3([r1, r2, h](const Vector3& p) {
        double q = std::sqrt(p.x * p.x + p.y * p.y);
        
        double b = (r1 - r2) / h;
        double a = std::sqrt(1.0 - b * b);
        double k = dot(Vector3(q, p.z, 0), Vector3(-b, a, 0));
        
        if (k < 0.0) return std::sqrt(q * q + p.z * p.z) - r1;
        if (k > a * h) return std::sqrt(q * q + (p.z - h) * (p.z - h)) - r2;
        
        return dot(Vector3(q, p.z, 0), Vector3(a, b, 0)) - r1;
    });
}

// Capped Cone
SDF3 cappedCone(const Vector3& a, const Vector3& b, double ra, double rb) {
    return SDF3([a, b, ra, rb](const Vector3& p) {
        double rba = rb - ra;
        double baba = dot(b - a, b - a);
        double papa = dot(p - a, p - a);
        double paba = dot(p - a, b - a) / baba;
        
        double x = std::sqrt(papa - paba * paba * baba);
        
        double cax = std::max(0.0, x - ((paba < 0.5) ? ra : rb));
        double cay = std::abs(paba - 0.5) - 0.5;
        
        double k = rba * rba + baba;
        double f = clamp((rba * (x - ra) + paba * baba) / k, 0.0, 1.0);
        
        double cbx = x - ra - f * rba;
        double cby = paba - f;
        
        double s = (cbx < 0.0 && cay < 0.0) ? -1.0 : 1.0;
        
        return s * std::sqrt(std::min(cax * cax + cay * cay * baba,
                                      cbx * cbx + cby * cby * baba));
    });
}

} // namespace sdf
