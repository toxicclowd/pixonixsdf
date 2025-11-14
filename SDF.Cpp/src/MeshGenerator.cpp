#include "sdf/MeshGenerator.h"
#include "sdf/MarchingCubes.h"
#include <cmath>
#include <algorithm>
#include <thread>
#include <mutex>
#include <atomic>
#include <iostream>
#include <chrono>

namespace sdf {

// Helper for clamp (C++17 compatible)
template<typename T>
static T clamp(T value, T min, T max) {
    return std::max(min, std::min(value, max));
}

std::vector<Vector3> MeshGenerator::generate(const SDF3& sdf, const Options& options) {
    auto startTime = std::chrono::high_resolution_clock::now();
    
    // Determine bounds
    Vector3 boundsMin, boundsMax;
    if (options.customBounds) {
        boundsMin = options.boundsMin;
        boundsMax = options.boundsMax;
    } else {
        auto bounds = estimateBounds(sdf);
        boundsMin = bounds.first;
        boundsMax = bounds.second;
    }
    
    if (options.verbose) {
        std::cout << "Bounds: (" << boundsMin << ") to (" << boundsMax << ")\n";
    }
    
    // Calculate step size
    double step = options.step;
    if (step == 0.0 && options.samples > 0) {
        double volume = (boundsMax.x - boundsMin.x) * 
                       (boundsMax.y - boundsMin.y) * 
                       (boundsMax.z - boundsMin.z);
        step = std::cbrt(volume / options.samples);
    }
    if (step == 0.0) {
        step = 0.1;  // Default step
    }
    
    // Calculate grid dimensions
    int nx = static_cast<int>(std::ceil((boundsMax.x - boundsMin.x) / step)) + 1;
    int ny = static_cast<int>(std::ceil((boundsMax.y - boundsMin.y) / step)) + 1;
    int nz = static_cast<int>(std::ceil((boundsMax.z - boundsMin.z) / step)) + 1;
    
    if (options.verbose) {
        std::cout << "Grid dimensions: " << nx << " x " << ny << " x " << nz << "\n";
        std::cout << "Step size: " << step << "\n";
    }
    
    // Adjust bounds to match grid
    boundsMax.x = boundsMin.x + (nx - 1) * step;
    boundsMax.y = boundsMin.y + (ny - 1) * step;
    boundsMax.z = boundsMin.z + (nz - 1) * step;
    
    // Create batches
    int batchSize = options.batchSize;
    std::vector<std::tuple<int, int, int, int, int, int>> batches;
    
    for (int z = 0; z < nz - 1; z += batchSize) {
        for (int y = 0; y < ny - 1; y += batchSize) {
            for (int x = 0; x < nx - 1; x += batchSize) {
                int x1 = std::min(x + batchSize, nx - 1);
                int y1 = std::min(y + batchSize, ny - 1);
                int z1 = std::min(z + batchSize, nz - 1);
                batches.push_back(std::make_tuple(x, x1, y, y1, z, z1));
            }
        }
    }
    
    if (options.verbose) {
        std::cout << "Processing " << batches.size() << " batches...\n";
    }
    
    // Determine worker count
    int workers = options.workers;
    if (workers == 0) {
        workers = std::thread::hardware_concurrency();
        if (workers == 0) workers = 4;
    }
    
    // Process batches in parallel
    std::vector<Vector3> allTriangles;
    std::mutex resultMutex;
    std::atomic<int> batchesProcessed(0);
    std::atomic<int> batchesSkipped(0);
    
    auto processBatchRange = [&](int start, int end) {
        std::vector<Vector3> localTriangles;
        
        for (int i = start; i < end; ++i) {
            auto [x0, x1, y0, y1, z0, z1] = batches[i];
            
            Vector3 batchMin(
                boundsMin.x + x0 * step,
                boundsMin.y + y0 * step,
                boundsMin.z + z0 * step
            );
            Vector3 batchMax(
                boundsMin.x + x1 * step,
                boundsMin.y + y1 * step,
                boundsMin.z + z1 * step
            );
            
            // Skip batch if far from surface (sparse sampling)
            if (options.sparse && canSkipBatch(sdf, batchMin, batchMax)) {
                batchesSkipped++;
                batchesProcessed++;
                continue;
            }
            
            // Generate grid points for this batch
            int batchNx = x1 - x0 + 1;
            int batchNy = y1 - y0 + 1;
            int batchNz = z1 - z0 + 1;
            
            std::vector<Vector3> gridPoints;
            gridPoints.reserve(batchNx * batchNy * batchNz);
            
            for (int bz = 0; bz < batchNz; ++bz) {
                for (int by = 0; by < batchNy; ++by) {
                    for (int bx = 0; bx < batchNx; ++bx) {
                        gridPoints.push_back(Vector3(
                            boundsMin.x + (x0 + bx) * step,
                            boundsMin.y + (y0 + by) * step,
                            boundsMin.z + (z0 + bz) * step
                        ));
                    }
                }
            }
            
            // Evaluate SDF at all grid points
            auto values = sdf.evaluate(gridPoints);
            
            // Run marching cubes
            std::array<int, 3> dims = {batchNx, batchNy, batchNz};
            auto batchTriangles = MarchingCubes::extractSurface(values, dims, 0.0);
            
            // Scale and translate triangles
            Vector3 scale(step, step, step);
            Vector3 offset = batchMin;
            
            for (auto& v : batchTriangles) {
                v = v * scale + offset;
            }
            
            localTriangles.insert(localTriangles.end(), 
                                 batchTriangles.begin(), 
                                 batchTriangles.end());
            
            batchesProcessed++;
        }
        
        // Merge results
        std::lock_guard<std::mutex> lock(resultMutex);
        allTriangles.insert(allTriangles.end(), 
                           localTriangles.begin(), 
                           localTriangles.end());
    };
    
    // Launch threads
    std::vector<std::thread> threads;
    int batchesPerWorker = (batches.size() + workers - 1) / workers;
    
    for (int i = 0; i < workers; ++i) {
        int start = i * batchesPerWorker;
        int end = std::min(start + batchesPerWorker, static_cast<int>(batches.size()));
        if (start < end) {
            threads.emplace_back(processBatchRange, start, end);
        }
    }
    
    // Wait for completion
    for (auto& thread : threads) {
        thread.join();
    }
    
    auto endTime = std::chrono::high_resolution_clock::now();
    auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(endTime - startTime);
    
    if (options.verbose) {
        std::cout << "Generated " << (allTriangles.size() / 3) << " triangles\n";
        std::cout << "Processed " << batchesProcessed << " batches (skipped " 
                  << batchesSkipped << ")\n";
        std::cout << "Time: " << (duration.count() / 1000.0) << " seconds\n";
    }
    
    return allTriangles;
}

std::pair<Vector3, Vector3> MeshGenerator::estimateBounds(const SDF3& sdf) {
    const int samples = 16;
    Vector3 boundsMin(-10, -10, -10);
    Vector3 boundsMax(10, 10, 10);
    
    double prevThreshold = -1.0;
    
    for (int iteration = 0; iteration < 32; ++iteration) {
        Vector3 step = (boundsMax - boundsMin) / (samples - 1);
        double threshold = step.length() / 2.0;
        
        if (std::abs(threshold - prevThreshold) < 1e-10) {
            break;
        }
        prevThreshold = threshold;
        
        // Sample points on a grid
        std::vector<Vector3> points;
        points.reserve(samples * samples * samples);
        
        for (int z = 0; z < samples; ++z) {
            for (int y = 0; y < samples; ++y) {
                for (int x = 0; x < samples; ++x) {
                    points.push_back(Vector3(
                        boundsMin.x + x * step.x,
                        boundsMin.y + y * step.y,
                        boundsMin.z + z * step.z
                    ));
                }
            }
        }
        
        // Evaluate SDF
        auto values = sdf.evaluate(points);
        
        // Find points near surface
        Vector3 newMin = boundsMax;
        Vector3 newMax = boundsMin;
        bool foundAny = false;
        
        for (size_t i = 0; i < points.size(); ++i) {
            if (std::abs(values[i]) <= threshold) {
                newMin = Vector3::min(newMin, points[i]);
                newMax = Vector3::max(newMax, points[i]);
                foundAny = true;
            }
        }
        
        if (!foundAny) {
            // No surface found, expand bounds
            Vector3 center = (boundsMin + boundsMax) * 0.5;
            Vector3 size = boundsMax - boundsMin;
            boundsMin = center - size;
            boundsMax = center + size;
        } else {
            // Add margin
            Vector3 margin = step * 0.5;
            boundsMin = newMin - margin;
            boundsMax = newMax + margin;
        }
    }
    
    return {boundsMin, boundsMax};
}

bool MeshGenerator::canSkipBatch(
    const SDF3& sdf,
    const Vector3& batchMin,
    const Vector3& batchMax
) {
    // Sample center and corners
    Vector3 center = (batchMin + batchMax) * 0.5;
    double radius = (batchMax - batchMin).length() / 2.0;
    
    // Check center distance
    double centerDist = sdf(center);
    if (std::abs(centerDist) <= radius) {
        return false;  // Surface might be in batch
    }
    
    // Check all 8 corners
    std::vector<Vector3> corners = {
        Vector3(batchMin.x, batchMin.y, batchMin.z),
        Vector3(batchMax.x, batchMin.y, batchMin.z),
        Vector3(batchMin.x, batchMax.y, batchMin.z),
        Vector3(batchMax.x, batchMax.y, batchMin.z),
        Vector3(batchMin.x, batchMin.y, batchMax.z),
        Vector3(batchMax.x, batchMin.y, batchMax.z),
        Vector3(batchMin.x, batchMax.y, batchMax.z),
        Vector3(batchMax.x, batchMax.y, batchMax.z)
    };
    
    auto values = sdf.evaluate(corners);
    
    // Check if all corners have the same sign
    bool allPositive = true;
    bool allNegative = true;
    for (double v : values) {
        if (v < 0) allPositive = false;
        if (v > 0) allNegative = false;
    }
    
    return allPositive || allNegative;
}

} // namespace sdf
