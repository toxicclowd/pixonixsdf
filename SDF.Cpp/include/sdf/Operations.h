#pragma once

#include "SDF3.h"
#include "Vector3.h"

namespace sdf {

// Boolean operations
SDF3 unionOp(const SDF3& a, const SDF3& b, double k = 0.0);
SDF3 intersection(const SDF3& a, const SDF3& b, double k = 0.0);
SDF3 difference(const SDF3& a, const SDF3& b, double k = 0.0);

// Transformations
SDF3 translate(const SDF3& sdf, const Vector3& offset);
SDF3 scale(const SDF3& sdf, double factor);
SDF3 scale(const SDF3& sdf, const Vector3& factor);
SDF3 rotate(const SDF3& sdf, double angle, const Vector3& axis);
SDF3 orient(const SDF3& sdf, const Vector3& axis);

// Deformations
SDF3 twist(const SDF3& sdf, double k);
SDF3 bend(const SDF3& sdf, double k);
SDF3 elongate(const SDF3& sdf, const Vector3& h);

// Modifiers
SDF3 dilate(const SDF3& sdf, double r);
SDF3 erode(const SDF3& sdf, double r);
SDF3 shell(const SDF3& sdf, double thickness);
SDF3 repeatOp(const SDF3& sdf, const Vector3& spacing, const Vector3& count = Vector3(1e9, 1e9, 1e9));

// Additional operations
SDF3 blend(const SDF3& a, const SDF3& b, double k = 0.5);
SDF3 circularArray(const SDF3& sdf, int count, double offset);

} // namespace sdf
