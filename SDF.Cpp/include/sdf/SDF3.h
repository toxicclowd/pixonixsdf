#pragma once

#include "Vector3.h"
#include <functional>
#include <memory>
#include <vector>
#include <string>

namespace sdf {

/**
 * @brief Base class for 3D Signed Distance Functions
 * 
 * This class represents a signed distance function that can be evaluated
 * at any point in 3D space. It supports operator overloading for boolean
 * operations and provides methods for mesh generation and export.
 */
class SDF3 {
public:
    using Function = std::function<double(const Vector3&)>;
    using VectorFunction = std::function<std::vector<double>(const std::vector<Vector3>&)>;

    // Constructors
    SDF3();
    explicit SDF3(Function func);
    explicit SDF3(VectorFunction vecFunc);

    // Evaluation
    double operator()(const Vector3& point) const;
    std::vector<double> evaluate(const std::vector<Vector3>& points) const;

    // Boolean operations (operators)
    SDF3 operator|(const SDF3& other) const;  // Union
    SDF3 operator&(const SDF3& other) const;  // Intersection
    SDF3 operator-(const SDF3& other) const;  // Difference

    // Smoothing parameter
    SDF3 k(double k_value) const;
    double getK() const { return k_; }

    // Mesh generation
    std::vector<Vector3> generate(
        double step = 0.0,
        const Vector3* boundsMin = nullptr,
        const Vector3* boundsMax = nullptr,
        int samples = 4194304,
        int workers = 0,
        int batchSize = 32,
        bool verbose = true,
        bool sparse = true
    ) const;

    // Save to file
    void save(
        const std::string& path,
        double step = 0.0,
        const Vector3* boundsMin = nullptr,
        const Vector3* boundsMax = nullptr,
        int samples = 4194304,
        int workers = 0,
        int batchSize = 32,
        bool verbose = true,
        bool sparse = true
    ) const;

    // Get the underlying function
    const VectorFunction& getFunction() const { return vecFunc_; }

protected:
    VectorFunction vecFunc_;
    double k_;

    // Helper to wrap single-point function as vector function
    static VectorFunction wrapFunction(Function func);
};

} // namespace sdf
