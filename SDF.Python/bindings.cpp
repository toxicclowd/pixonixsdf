#include <pybind11/pybind11.h>
#include <pybind11/functional.h>
#include <pybind11/stl.h>
#include <sdf/Vector3.h>
#include <sdf/SDF3.h>
#include <sdf/Primitives.h>
#include <sdf/Operations.h>
#include <sdf/Constants.h>

namespace py = pybind11;
using namespace sdf;

PYBIND11_MODULE(sdf_cpp, m) {
    m.doc() = "C++ SDF library Python bindings";

    // Constants
    m.attr("PI") = PI;
    m.attr("TAU") = TAU;
    
    // Vector3 class
    py::class_<Vector3>(m, "Vector3")
        .def(py::init<>())
        .def(py::init<double, double, double>())
        .def(py::init<double>())
        .def_readwrite("x", &Vector3::x)
        .def_readwrite("y", &Vector3::y)
        .def_readwrite("z", &Vector3::z)
        .def("__add__", &Vector3::operator+)
        .def("__sub__", &Vector3::operator-)
        .def("__mul__", py::overload_cast<double>(&Vector3::operator*, py::const_))
        .def("__truediv__", py::overload_cast<double>(&Vector3::operator/, py::const_))
        .def("__neg__", &Vector3::operator-)
        .def("length", &Vector3::length)
        .def("normalized", &Vector3::normalized)
        .def("dot", &Vector3::dot)
        .def("cross", &Vector3::cross)
        .def("__repr__", [](const Vector3& v) {
            return "Vector3(" + std::to_string(v.x) + ", " + 
                   std::to_string(v.y) + ", " + std::to_string(v.z) + ")";
        });
    
    // Direction vector constants
    m.attr("X") = py::cast(X);
    m.attr("Y") = py::cast(Y);
    m.attr("Z") = py::cast(Z);
    m.attr("ORIGIN") = py::cast(ORIGIN);
    m.attr("UP") = py::cast(UP);
    
    // SDF3 class
    py::class_<SDF3>(m, "SDF3")
        .def("__call__", &SDF3::operator())
        .def("__or__", &SDF3::operator|, "Union operation")
        .def("__and__", &SDF3::operator&, "Intersection operation")
        .def("__sub__", &SDF3::operator-, "Difference operation")
        .def("k", &SDF3::k, "Set smoothing parameter")
        .def("save", [](const SDF3& self, const std::string& path) {
            self.save(path);
        }, "Save to STL file")
        .def("generate", [](const SDF3& self) {
            return self.generate();
        }, "Generate mesh vertices");
    
    // Primitives
    m.def("sphere", py::overload_cast<>(&sphere), "Create a unit sphere");
    m.def("sphere", py::overload_cast<double>(&sphere), "Create a sphere with given radius");
    m.def("sphere", py::overload_cast<double, const Vector3&>(&sphere), 
          "Create a sphere with radius and center");
    
    m.def("box", py::overload_cast<double>(&box), "Create a unit cube");
    m.def("box", py::overload_cast<const Vector3&>(&box), "Create a box with given size");
    m.def("box", py::overload_cast<double, const Vector3&>(&box), 
          "Create a cube at position");
    m.def("box", py::overload_cast<const Vector3&, const Vector3&>(&box), 
          "Create a box with size and center");
    
    m.def("torus", &torus, "Create a torus");
    m.def("capsule", &capsule, "Create a capsule");
    m.def("capped_cylinder", &cappedCylinder, "Create a capped cylinder");
    m.def("cylinder", &cylinder, "Create an infinite cylinder");
    m.def("ellipsoid", &ellipsoid, "Create an ellipsoid");
    m.def("rounded_box", &roundedBox, "Create a rounded box");
    
    m.def("plane", py::overload_cast<>(&plane), "Create a plane");
    m.def("plane", py::overload_cast<const Vector3&, const Vector3&>(&plane), 
          "Create a plane with normal and point");
    
    m.def("slab", &slab, "Create a slab",
          py::arg("x0") = -1e9, py::arg("x1") = 1e9,
          py::arg("y0") = -1e9, py::arg("y1") = 1e9,
          py::arg("z0") = -1e9, py::arg("z1") = 1e9);
    
    // Operations
    m.def("union_op", &unionOp, "Union operation", 
          py::arg("a"), py::arg("b"), py::arg("k") = 0.0);
    m.def("intersection", &intersection, "Intersection operation",
          py::arg("a"), py::arg("b"), py::arg("k") = 0.0);
    m.def("difference", &difference, "Difference operation",
          py::arg("a"), py::arg("b"), py::arg("k") = 0.0);
    
    m.def("translate", &translate, "Translate an SDF");
    m.def("scale", py::overload_cast<const SDF3&, double>(&scale), "Scale an SDF uniformly");
    m.def("scale", py::overload_cast<const SDF3&, const Vector3&>(&scale), "Scale an SDF non-uniformly");
    m.def("rotate", &rotate, "Rotate an SDF");
    m.def("orient", &orient, "Orient an SDF");
    
    m.def("twist", &twist, "Twist an SDF");
    m.def("bend", &bend, "Bend an SDF");
    m.def("elongate", &elongate, "Elongate an SDF");
    
    m.def("dilate", &dilate, "Dilate (expand) an SDF");
    m.def("erode", &erode, "Erode (shrink) an SDF");
    m.def("shell", &shell, "Create a shell from an SDF");
    m.def("repeat", &repeatOp, "Repeat an SDF in space");
    m.def("blend", &blend, "Blend two SDFs",
          py::arg("a"), py::arg("b"), py::arg("k") = 0.5);
    m.def("circular_array", &circularArray, "Create circular array of an SDF");
}
