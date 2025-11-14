#include <sdf/Primitives.h>
#include <sdf/Operations.h>
#include <sdf/Constants.h>
#include <iostream>

using namespace sdf;

int main() {
    std::cout << "Generating CSG example...\n";
    
    // Create the classic CSG example
    auto f = sphere(1.0) & box(1.5);
    auto c = cylinder(0.5);
    f = f - (orient(c, X) | orient(c, Y) | orient(c, Z));
    
    f.save("csg_cpp.stl");
    
    std::cout << "CSG example generated successfully!\n";
    return 0;
}
