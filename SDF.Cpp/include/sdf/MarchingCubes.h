#pragma once

#include "Vector3.h"
#include <vector>
#include <array>

namespace sdf {

/**
 * @brief Marching Cubes algorithm for surface extraction
 */
class MarchingCubes {
public:
    /**
     * @brief Extract triangles from a 3D volume using marching cubes
     * 
     * @param volume 3D array of signed distance values
     * @param dims Dimensions of the volume (nx, ny, nz)
     * @param level Iso-level (typically 0.0 for signed distance fields)
     * @return Vector of triangle vertices (each 3 consecutive points form a triangle)
     */
    static std::vector<Vector3> extractSurface(
        const std::vector<double>& volume,
        const std::array<int, 3>& dims,
        double level = 0.0
    );

private:
    // Lookup tables for marching cubes
    static const int edgeTable[256];
    static const int triTable[256][16];

    // Helper functions
    static Vector3 vertexInterp(
        double isolevel,
        const Vector3& p1, const Vector3& p2,
        double v1, double v2
    );

    static int getIndex(int x, int y, int z, const std::array<int, 3>& dims);
};

} // namespace sdf
