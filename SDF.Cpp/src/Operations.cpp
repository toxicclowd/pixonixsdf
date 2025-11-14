#include "sdf/Operations.h"
#include "sdf/Constants.h"
#include <cmath>
#include <algorithm>

namespace sdf {

// Helper for clamp (C++17 compatible)
template<typename T>
static T clamp(T value, T min, T max) {
    return std::max(min, std::min(value, max));
}

// Boolean operations
SDF3 unionOp(const SDF3& a, const SDF3& b, double k) {
    if (k > 0.0) {
        return a.k(k) | b.k(k);
    }
    return a | b;
}

SDF3 intersection(const SDF3& a, const SDF3& b, double k) {
    if (k > 0.0) {
        return a.k(k) & b.k(k);
    }
    return a & b;
}

SDF3 difference(const SDF3& a, const SDF3& b, double k) {
    if (k > 0.0) {
        return a.k(k) - b.k(k);
    }
    return a - b;
}

// Translate
SDF3 translate(const SDF3& sdf, const Vector3& offset) {
    auto func = sdf.getFunction();
    return SDF3([func, offset](const std::vector<Vector3>& points) {
        std::vector<Vector3> translated;
        translated.reserve(points.size());
        for (const auto& p : points) {
            translated.push_back(p - offset);
        }
        return func(translated);
    });
}

// Scale
SDF3 scale(const SDF3& sdf, double factor) {
    auto func = sdf.getFunction();
    return SDF3([func, factor](const std::vector<Vector3>& points) {
        std::vector<Vector3> scaled;
        scaled.reserve(points.size());
        for (const auto& p : points) {
            scaled.push_back(p / factor);
        }
        auto result = func(scaled);
        for (auto& r : result) {
            r *= factor;
        }
        return result;
    });
}

SDF3 scale(const SDF3& sdf, const Vector3& factor) {
    auto func = sdf.getFunction();
    return SDF3([func, factor](const std::vector<Vector3>& points) {
        std::vector<Vector3> scaled;
        scaled.reserve(points.size());
        for (const auto& p : points) {
            scaled.push_back(p / factor);
        }
        return func(scaled);
    });
}

// Rotate
SDF3 rotate(const SDF3& sdf, double angle, const Vector3& axis) {
    Vector3 a = axis.normalized();
    double c = std::cos(angle);
    double s = std::sin(angle);
    double t = 1.0 - c;
    
    // Rotation matrix (transposed for inverse rotation)
    auto rotatePoint = [a, c, s, t](const Vector3& p) {
        double x = p.x * (t * a.x * a.x + c) + 
                   p.y * (t * a.x * a.y + s * a.z) + 
                   p.z * (t * a.x * a.z - s * a.y);
        double y = p.x * (t * a.x * a.y - s * a.z) + 
                   p.y * (t * a.y * a.y + c) + 
                   p.z * (t * a.y * a.z + s * a.x);
        double z = p.x * (t * a.x * a.z + s * a.y) + 
                   p.y * (t * a.y * a.z - s * a.x) + 
                   p.z * (t * a.z * a.z + c);
        return Vector3(x, y, z);
    };
    
    auto func = sdf.getFunction();
    return SDF3([func, rotatePoint](const std::vector<Vector3>& points) {
        std::vector<Vector3> rotated;
        rotated.reserve(points.size());
        for (const auto& p : points) {
            rotated.push_back(rotatePoint(p));
        }
        return func(rotated);
    });
}

// Orient
SDF3 orient(const SDF3& sdf, const Vector3& targetAxis) {
    Vector3 src = Z;
    Vector3 dst = targetAxis.normalized();
    
    // If already aligned, no rotation needed
    if ((src - dst).lengthSquared() < 1e-10) {
        return sdf;
    }
    
    // If opposite direction, rotate 180 degrees around perpendicular axis
    if ((src + dst).lengthSquared() < 1e-10) {
        Vector3 perp = (std::abs(src.x) < 0.9) ? Vector3(1, 0, 0) : Vector3(0, 1, 0);
        return rotate(sdf, PI, perp);
    }
    
    // Calculate rotation axis and angle
    Vector3 axis = src.cross(dst).normalized();
    double angle = std::acos(clamp(src.dot(dst), -1.0, 1.0));
    
    return rotate(sdf, angle, axis);
}

// Twist
SDF3 twist(const SDF3& sdf, double k) {
    auto func = sdf.getFunction();
    return SDF3([func, k](const std::vector<Vector3>& points) {
        std::vector<Vector3> twisted;
        twisted.reserve(points.size());
        for (const auto& p : points) {
            double c = std::cos(k * p.z);
            double s = std::sin(k * p.z);
            twisted.push_back(Vector3(
                c * p.x - s * p.y,
                s * p.x + c * p.y,
                p.z
            ));
        }
        return func(twisted);
    });
}

// Bend
SDF3 bend(const SDF3& sdf, double k) {
    auto func = sdf.getFunction();
    return SDF3([func, k](const std::vector<Vector3>& points) {
        std::vector<Vector3> bent;
        bent.reserve(points.size());
        for (const auto& p : points) {
            double c = std::cos(k * p.x);
            double s = std::sin(k * p.x);
            bent.push_back(Vector3(
                c * p.x - s * p.y,
                s * p.x + c * p.y,
                p.z
            ));
        }
        return func(bent);
    });
}

// Elongate
SDF3 elongate(const SDF3& sdf, const Vector3& h) {
    auto func = sdf.getFunction();
    return SDF3([func, h](const std::vector<Vector3>& points) {
        std::vector<Vector3> elongated;
        elongated.reserve(points.size());
        for (const auto& p : points) {
            Vector3 q = Vector3::abs(p) - h;
            elongated.push_back(Vector3(
                std::max(q.x, 0.0) * (p.x >= 0 ? 1.0 : -1.0),
                std::max(q.y, 0.0) * (p.y >= 0 ? 1.0 : -1.0),
                std::max(q.z, 0.0) * (p.z >= 0 ? 1.0 : -1.0)
            ));
        }
        auto result = func(elongated);
        
        // Adjust for exterior distance
        for (size_t i = 0; i < points.size(); ++i) {
            Vector3 q = Vector3::abs(points[i]) - h;
            Vector3 maxQ = Vector3::max(q, Vector3(0, 0, 0));
            result[i] += maxQ.length();
        }
        return result;
    });
}

// Dilate
SDF3 dilate(const SDF3& sdf, double r) {
    auto func = sdf.getFunction();
    return SDF3([func, r](const std::vector<Vector3>& points) {
        auto result = func(points);
        for (auto& d : result) {
            d -= r;
        }
        return result;
    });
}

// Erode
SDF3 erode(const SDF3& sdf, double r) {
    return dilate(sdf, -r);
}

// Shell
SDF3 shell(const SDF3& sdf, double thickness) {
    auto func = sdf.getFunction();
    return SDF3([func, thickness](const std::vector<Vector3>& points) {
        auto result = func(points);
        for (auto& d : result) {
            d = std::abs(d) - thickness;
        }
        return result;
    });
}

// Repeat
SDF3 repeatOp(const SDF3& sdf, const Vector3& spacing, const Vector3& count) {
    auto func = sdf.getFunction();
    return SDF3([func, spacing, count](const std::vector<Vector3>& points) {
        std::vector<Vector3> repeated;
        repeated.reserve(points.size());
        
        for (const auto& p : points) {
            Vector3 q = p;
            
            // Apply modulo for each axis if count is not infinite
            if (count.x < 1e8) {
                double cx = std::round(p.x / spacing.x);
                cx = clamp(cx, -count.x, count.x);
                q.x = p.x - cx * spacing.x;
            } else {
                q.x = p.x - std::round(p.x / spacing.x) * spacing.x;
            }
            
            if (count.y < 1e8) {
                double cy = std::round(p.y / spacing.y);
                cy = clamp(cy, -count.y, count.y);
                q.y = p.y - cy * spacing.y;
            } else {
                q.y = p.y - std::round(p.y / spacing.y) * spacing.y;
            }
            
            if (count.z < 1e8) {
                double cz = std::round(p.z / spacing.z);
                cz = clamp(cz, -count.z, count.z);
                q.z = p.z - cz * spacing.z;
            } else {
                q.z = p.z - std::round(p.z / spacing.z) * spacing.z;
            }
            
            repeated.push_back(q);
        }
        
        return func(repeated);
    });
}

// Blend
SDF3 blend(const SDF3& a, const SDF3& b, double k) {
    auto funcA = a.getFunction();
    auto funcB = b.getFunction();
    
    return SDF3([funcA, funcB, k](const std::vector<Vector3>& points) {
        auto ra = funcA(points);
        auto rb = funcB(points);
        std::vector<double> result;
        result.reserve(points.size());
        
        for (size_t i = 0; i < points.size(); ++i) {
            result.push_back(ra[i] * (1.0 - k) + rb[i] * k);
        }
        
        return result;
    });
}

// Circular Array
SDF3 circularArray(const SDF3& sdf, int count, double offset) {
    auto func = sdf.getFunction();
    
    return SDF3([func, count, offset](const std::vector<Vector3>& points) {
        std::vector<double> result(points.size(), 1e9);
        
        for (int i = 0; i < count; ++i) {
            double angle = 2.0 * PI * i / count;
            double c = std::cos(angle);
            double s = std::sin(angle);
            
            std::vector<Vector3> transformed;
            transformed.reserve(points.size());
            
            for (const auto& p : points) {
                // Rotate around Z axis and translate
                double x = c * (p.x - offset) - s * p.y + offset;
                double y = s * (p.x - offset) + c * p.y;
                transformed.push_back(Vector3(x, y, p.z));
            }
            
            auto distances = func(transformed);
            for (size_t j = 0; j < points.size(); ++j) {
                result[j] = std::min(result[j], distances[j]);
            }
        }
        
        return result;
    });
}

} // namespace sdf
