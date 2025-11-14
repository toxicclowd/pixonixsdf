#pragma once

#include "SDF3.h"
#include "Vector3.h"
#include <vector>
#include <array>
#include <tuple>

namespace sdf {

/**
 * @brief Mesh generation from SDFs
 */
class MeshGenerator {
public:
    struct Options {
        double step = 0.0;
        Vector3 boundsMin;
        Vector3 boundsMax;
        bool customBounds = false;
        int samples = 4194304;  // 2^22
        int workers = 0;         // 0 = auto-detect
        int batchSize = 32;
        bool verbose = true;
        bool sparse = true;
    };

    /**
     * @brief Generate mesh from SDF
     * 
     * @param sdf The signed distance function
     * @param options Generation options
     * @return Vector of triangle vertices
     */
    static std::vector<Vector3> generate(const SDF3& sdf, const Options& options);

private:
    // Estimate bounds of the SDF
    static std::pair<Vector3, Vector3> estimateBounds(const SDF3& sdf);

    // Check if a batch can be skipped (sparse sampling)
    static bool canSkipBatch(
        const SDF3& sdf,
        const Vector3& batchMin,
        const Vector3& batchMax
    );

    // Process a single batch
    static std::vector<Vector3> processBatch(
        const SDF3& sdf,
        const std::vector<Vector3>& gridPoints,
        const std::array<int, 3>& dims,
        const Vector3& scale,
        const Vector3& offset
    );

    // Generate grid points for a batch
    static std::vector<Vector3> generateGridPoints(
        const Vector3& min,
        const Vector3& max,
        const std::array<int, 3>& dims
    );
};

} // namespace sdf
