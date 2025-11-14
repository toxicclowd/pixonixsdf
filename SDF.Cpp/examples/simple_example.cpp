#include <sdf/Primitives.h>
#include <sdf/Operations.h>
#include <sdf/Constants.h>
#include <iostream>

using namespace sdf;

int main() {
    std::cout << "Generating simple sphere...\n";
    
    auto s = sphere(1.0);
    s.save("sphere.stl");
    
    std::cout << "Sphere generated successfully!\n";
    return 0;
}
