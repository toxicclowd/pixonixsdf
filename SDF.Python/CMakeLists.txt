cmake_minimum_required(VERSION 3.15)
project(sdf_python_bindings)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Find Python and pybind11
find_package(Python3 COMPONENTS Interpreter Development REQUIRED)
find_package(pybind11 CONFIG REQUIRED)

# Add the C++ SDF library
add_subdirectory(../SDF.Cpp ${CMAKE_CURRENT_BINARY_DIR}/sdf_cpp)

# Create Python module
pybind11_add_module(sdf_cpp bindings.cpp)

# Link against the SDF library
target_link_libraries(sdf_cpp PRIVATE sdf)

# Set output directory
set_target_properties(sdf_cpp PROPERTIES
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}"
)

# Install target
install(TARGETS sdf_cpp
    LIBRARY DESTINATION .
)
