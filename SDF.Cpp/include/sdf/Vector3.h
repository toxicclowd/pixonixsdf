#pragma once

#include <cmath>
#include <algorithm>
#include <ostream>

namespace sdf {

/**
 * @brief 3D vector class for SDF operations
 */
class Vector3 {
public:
    double x, y, z;

    // Constructors
    Vector3() : x(0), y(0), z(0) {}
    Vector3(double x, double y, double z) : x(x), y(y), z(z) {}
    Vector3(double value) : x(value), y(value), z(value) {}

    // Basic operations
    Vector3 operator+(const Vector3& other) const {
        return Vector3(x + other.x, y + other.y, z + other.z);
    }

    Vector3 operator-(const Vector3& other) const {
        return Vector3(x - other.x, y - other.y, z - other.z);
    }

    Vector3 operator*(double scalar) const {
        return Vector3(x * scalar, y * scalar, z * scalar);
    }

    Vector3 operator*(const Vector3& other) const {
        return Vector3(x * other.x, y * other.y, z * other.z);
    }

    Vector3 operator/(double scalar) const {
        return Vector3(x / scalar, y / scalar, z / scalar);
    }

    Vector3 operator/(const Vector3& other) const {
        return Vector3(x / other.x, y / other.y, z / other.z);
    }

    Vector3 operator-() const {
        return Vector3(-x, -y, -z);
    }

    Vector3& operator+=(const Vector3& other) {
        x += other.x;
        y += other.y;
        z += other.z;
        return *this;
    }

    Vector3& operator-=(const Vector3& other) {
        x -= other.x;
        y -= other.y;
        z -= other.z;
        return *this;
    }

    Vector3& operator*=(double scalar) {
        x *= scalar;
        y *= scalar;
        z *= scalar;
        return *this;
    }

    Vector3& operator/=(double scalar) {
        x /= scalar;
        y /= scalar;
        z /= scalar;
        return *this;
    }

    // Comparison
    bool operator==(const Vector3& other) const {
        return x == other.x && y == other.y && z == other.z;
    }

    bool operator!=(const Vector3& other) const {
        return !(*this == other);
    }

    // Vector operations
    double length() const {
        return std::sqrt(x * x + y * y + z * z);
    }

    double lengthSquared() const {
        return x * x + y * y + z * z;
    }

    Vector3 normalized() const {
        double len = length();
        if (len < 1e-10) return Vector3(0, 0, 0);
        return *this / len;
    }

    double dot(const Vector3& other) const {
        return x * other.x + y * other.y + z * other.z;
    }

    Vector3 cross(const Vector3& other) const {
        return Vector3(
            y * other.z - z * other.y,
            z * other.x - x * other.z,
            x * other.y - y * other.x
        );
    }

    // Static utility functions
    static Vector3 min(const Vector3& a, const Vector3& b) {
        return Vector3(
            std::min(a.x, b.x),
            std::min(a.y, b.y),
            std::min(a.z, b.z)
        );
    }

    static Vector3 max(const Vector3& a, const Vector3& b) {
        return Vector3(
            std::max(a.x, b.x),
            std::max(a.y, b.y),
            std::max(a.z, b.z)
        );
    }

    static Vector3 abs(const Vector3& v) {
        return Vector3(std::abs(v.x), std::abs(v.y), std::abs(v.z));
    }

    static Vector3 clamp(const Vector3& v, const Vector3& min, const Vector3& max) {
        return Vector3(
            std::max(min.x, std::min(v.x, max.x)),
            std::max(min.y, std::min(v.y, max.y)),
            std::max(min.z, std::min(v.z, max.z))
        );
    }

    // Array access
    double& operator[](int i) {
        return (&x)[i];
    }

    const double& operator[](int i) const {
        return (&x)[i];
    }
};

// Stream output
inline std::ostream& operator<<(std::ostream& os, const Vector3& v) {
    os << "(" << v.x << ", " << v.y << ", " << v.z << ")";
    return os;
}

// Scalar multiplication (scalar * vector)
inline Vector3 operator*(double scalar, const Vector3& v) {
    return v * scalar;
}

} // namespace sdf
