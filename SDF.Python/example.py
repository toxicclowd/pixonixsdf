"""
Example usage of the C++ SDF library from Python
"""

import sdf_cpp

def main():
    print("Creating CSG example with C++ backend...")
    
    # Create shapes using Python bindings to C++ library
    f = sdf_cpp.sphere(1.0) & sdf_cpp.box(1.5)
    
    # Create cylinders
    c = sdf_cpp.cylinder(0.5)
    
    # Apply transformations and boolean operations
    f = f - (sdf_cpp.orient(c, sdf_cpp.X) | 
             sdf_cpp.orient(c, sdf_cpp.Y) | 
             sdf_cpp.orient(c, sdf_cpp.Z))
    
    # Save to file
    print("Generating mesh...")
    f.save("csg_python_cpp.stl")
    print("Done! Saved to csg_python_cpp.stl")

if __name__ == '__main__':
    main()
