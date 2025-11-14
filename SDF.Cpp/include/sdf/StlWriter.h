#pragma once

#include "Vector3.h"
#include <vector>
#include <string>

namespace sdf {

/**
 * @brief Binary STL file writer
 */
class StlWriter {
public:
    /**
     * @brief Write mesh to binary STL file
     * 
     * @param path Output file path
     * @param vertices Triangle vertices (each 3 consecutive points form a triangle)
     */
    static void writeBinaryStl(const std::string& path, const std::vector<Vector3>& vertices);

    /**
     * @brief Write mesh to ASCII STL file
     * 
     * @param path Output file path
     * @param vertices Triangle vertices (each 3 consecutive points form a triangle)
     */
    static void writeAsciiStl(const std::string& path, const std::vector<Vector3>& vertices);

private:
    // Calculate normal for a triangle
    static Vector3 calculateNormal(const Vector3& v1, const Vector3& v2, const Vector3& v3);
};

} // namespace sdf
