cmake_minimum_required(VERSION 3.15)

# Simple example
add_executable(simple_example simple_example.cpp)
target_link_libraries(simple_example PRIVATE sdf)

# CSG example
add_executable(csg_example csg_example.cpp)
target_link_libraries(csg_example PRIVATE sdf)

# Operations example
add_executable(operations_example operations_example.cpp)
target_link_libraries(operations_example PRIVATE sdf)
