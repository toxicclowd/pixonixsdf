#pragma once

#include "Vector3.h"
#include <cmath>

namespace sdf {

// Mathematical constants
constexpr double PI = 3.14159265358979323846;
constexpr double TAU = 2.0 * PI;

// Direction vectors
const Vector3 ORIGIN(0, 0, 0);
const Vector3 X(1, 0, 0);
const Vector3 Y(0, 1, 0);
const Vector3 Z(0, 0, 1);
const Vector3 UP = Z;

// Conversion functions
inline double radians(double degrees) {
    return degrees * PI / 180.0;
}

inline double degrees(double radians) {
    return radians * 180.0 / PI;
}

} // namespace sdf
