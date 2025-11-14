#include "sdf/SDF3.h"
#include "sdf/MeshGenerator.h"
#include "sdf/StlWriter.h"
#include <algorithm>
#include <cmath>

namespace sdf {

// Helper for clamp (C++17 compatible)
template<typename T>
static T clamp(T value, T min, T max) {
    return std::max(min, std::min(value, max));
}

SDF3::SDF3() : k_(0.0) {
    vecFunc_ = [](const std::vector<Vector3>&) {
        return std::vector<double>();
    };
}

SDF3::SDF3(Function func) : k_(0.0) {
    vecFunc_ = wrapFunction(func);
}

SDF3::SDF3(VectorFunction vecFunc) : vecFunc_(vecFunc), k_(0.0) {}

double SDF3::operator()(const Vector3& point) const {
    auto result = vecFunc_({point});
    return result.empty() ? 0.0 : result[0];
}

std::vector<double> SDF3::evaluate(const std::vector<Vector3>& points) const {
    return vecFunc_(points);
}

SDF3::VectorFunction SDF3::wrapFunction(Function func) {
    return [func](const std::vector<Vector3>& points) {
        std::vector<double> results;
        results.reserve(points.size());
        for (const auto& p : points) {
            results.push_back(func(p));
        }
        return results;
    };
}

// Boolean operations
SDF3 SDF3::operator|(const SDF3& other) const {
    double k = std::max(k_, other.k_);
    auto thisFunc = vecFunc_;
    auto otherFunc = other.vecFunc_;
    
    return SDF3([thisFunc, otherFunc, k](const std::vector<Vector3>& points) {
        auto a = thisFunc(points);
        auto b = otherFunc(points);
        std::vector<double> result;
        result.reserve(points.size());
        
        if (k == 0.0) {
            // Hard union
            for (size_t i = 0; i < points.size(); ++i) {
                result.push_back(std::min(a[i], b[i]));
            }
        } else {
            // Smooth union
            for (size_t i = 0; i < points.size(); ++i) {
                double h = clamp(0.5 + 0.5 * (b[i] - a[i]) / k, 0.0, 1.0);
                double d = b[i] * (1.0 - h) + a[i] * h - k * h * (1.0 - h);
                result.push_back(d);
            }
        }
        
        return result;
    });
}

SDF3 SDF3::operator&(const SDF3& other) const {
    double k = std::max(k_, other.k_);
    auto thisFunc = vecFunc_;
    auto otherFunc = other.vecFunc_;
    
    return SDF3([thisFunc, otherFunc, k](const std::vector<Vector3>& points) {
        auto a = thisFunc(points);
        auto b = otherFunc(points);
        std::vector<double> result;
        result.reserve(points.size());
        
        if (k == 0.0) {
            // Hard intersection
            for (size_t i = 0; i < points.size(); ++i) {
                result.push_back(std::max(a[i], b[i]));
            }
        } else {
            // Smooth intersection
            for (size_t i = 0; i < points.size(); ++i) {
                double h = clamp(0.5 - 0.5 * (b[i] - a[i]) / k, 0.0, 1.0);
                double d = b[i] * (1.0 - h) + a[i] * h + k * h * (1.0 - h);
                result.push_back(d);
            }
        }
        
        return result;
    });
}

SDF3 SDF3::operator-(const SDF3& other) const {
    double k = std::max(k_, other.k_);
    auto thisFunc = vecFunc_;
    auto otherFunc = other.vecFunc_;
    
    return SDF3([thisFunc, otherFunc, k](const std::vector<Vector3>& points) {
        auto a = thisFunc(points);
        auto b = otherFunc(points);
        std::vector<double> result;
        result.reserve(points.size());
        
        if (k == 0.0) {
            // Hard difference
            for (size_t i = 0; i < points.size(); ++i) {
                result.push_back(std::max(a[i], -b[i]));
            }
        } else {
            // Smooth difference
            for (size_t i = 0; i < points.size(); ++i) {
                double h = clamp(0.5 - 0.5 * (a[i] + b[i]) / k, 0.0, 1.0);
                double d = a[i] * (1.0 - h) + (-b[i]) * h + k * h * (1.0 - h);
                result.push_back(d);
            }
        }
        
        return result;
    });
}

SDF3 SDF3::k(double k_value) const {
    SDF3 result = *this;
    result.k_ = k_value;
    return result;
}

std::vector<Vector3> SDF3::generate(
    double step,
    const Vector3* boundsMin,
    const Vector3* boundsMax,
    int samples,
    int workers,
    int batchSize,
    bool verbose,
    bool sparse
) const {
    MeshGenerator::Options options;
    options.step = step;
    if (boundsMin && boundsMax) {
        options.boundsMin = *boundsMin;
        options.boundsMax = *boundsMax;
        options.customBounds = true;
    }
    options.samples = samples;
    options.workers = workers;
    options.batchSize = batchSize;
    options.verbose = verbose;
    options.sparse = sparse;
    
    return MeshGenerator::generate(*this, options);
}

void SDF3::save(
    const std::string& path,
    double step,
    const Vector3* boundsMin,
    const Vector3* boundsMax,
    int samples,
    int workers,
    int batchSize,
    bool verbose,
    bool sparse
) const {
    auto vertices = generate(step, boundsMin, boundsMax, samples, workers, batchSize, verbose, sparse);
    StlWriter::writeBinaryStl(path, vertices);
}

} // namespace sdf
