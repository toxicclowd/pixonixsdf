cmake_minimum_required(VERSION 3.15)
project(SDF VERSION 1.0.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

# Options
option(BUILD_SHARED_LIBS "Build shared libraries" ON)
option(BUILD_EXAMPLES "Build examples" ON)
option(BUILD_PYTHON_BINDINGS "Build Python bindings" ON)
option(BUILD_DOTNET_BINDINGS "Build .NET bindings" ON)

# Find required packages
find_package(Threads REQUIRED)

# Core library sources
set(SDF_SOURCES
    src/Vector3.cpp
    src/SDF3.cpp
    src/Primitives.cpp
    src/Operations.cpp
    src/MarchingCubes.cpp
    src/MeshGenerator.cpp
    src/StlWriter.cpp
)

# Core library headers
set(SDF_HEADERS
    include/sdf/Vector3.h
    include/sdf/SDF3.h
    include/sdf/Primitives.h
    include/sdf/Operations.h
    include/sdf/MarchingCubes.h
    include/sdf/MeshGenerator.h
    include/sdf/StlWriter.h
    include/sdf/Constants.h
)

# Create the main library
add_library(sdf ${SDF_SOURCES} ${SDF_HEADERS})
target_include_directories(sdf PUBLIC
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
    $<INSTALL_INTERFACE:include>
)
target_link_libraries(sdf PUBLIC Threads::Threads)

# Set compiler warnings
if(MSVC)
    target_compile_options(sdf PRIVATE /W4)
else()
    target_compile_options(sdf PRIVATE -Wall -Wextra -Wpedantic)
endif()

# Examples
if(BUILD_EXAMPLES)
    add_subdirectory(examples)
endif()

# Python bindings
if(BUILD_PYTHON_BINDINGS)
    find_package(Python3 COMPONENTS Interpreter Development)
    if(Python3_FOUND)
        # We'll add pybind11 integration later
        message(STATUS "Python bindings will be built")
    else()
        message(WARNING "Python not found, skipping Python bindings")
    endif()
endif()

# Install targets
install(TARGETS sdf
    EXPORT SDFTargets
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
    RUNTIME DESTINATION bin
    INCLUDES DESTINATION include
)

install(DIRECTORY include/sdf DESTINATION include)

# Export targets
install(EXPORT SDFTargets
    FILE SDFTargets.cmake
    NAMESPACE SDF::
    DESTINATION lib/cmake/SDF
)

# Create config files
include(CMakePackageConfigHelpers)
write_basic_package_version_file(
    "${CMAKE_CURRENT_BINARY_DIR}/SDFConfigVersion.cmake"
    VERSION ${PROJECT_VERSION}
    COMPATIBILITY AnyNewerVersion
)

configure_package_config_file(
    "${CMAKE_CURRENT_SOURCE_DIR}/cmake/SDFConfig.cmake.in"
    "${CMAKE_CURRENT_BINARY_DIR}/SDFConfig.cmake"
    INSTALL_DESTINATION lib/cmake/SDF
)

install(FILES
    "${CMAKE_CURRENT_BINARY_DIR}/SDFConfig.cmake"
    "${CMAKE_CURRENT_BINARY_DIR}/SDFConfigVersion.cmake"
    DESTINATION lib/cmake/SDF
)
