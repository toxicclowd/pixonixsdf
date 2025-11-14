#include <sdf/Primitives.h>
#include <sdf/Operations.h>
#include <sdf/Constants.h>
#include <iostream>

using namespace sdf;

int main() {
    std::cout << "Generating operations example...\n";
    
    // Create a twisted box
    auto twistedBox = twist(box(Vector3(0.5, 0.5, 2.0)), PI / 2);
    
    // Create a torus
    auto t = torus(1.0, 0.2);
    
    // Combine them
    auto scene = twistedBox | rotate(t, PI / 2, X);
    scene = translate(scene, Vector3(0, 0, 0));
    
    scene.save("operations_cpp.stl");
    
    std::cout << "Operations example generated successfully!\n";
    return 0;
}
