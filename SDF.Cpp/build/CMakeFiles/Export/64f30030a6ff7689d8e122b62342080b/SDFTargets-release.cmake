#----------------------------------------------------------------
# Generated CMake target import file for configuration "Release".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "SDF::sdf" for configuration "Release"
set_property(TARGET SDF::sdf APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(SDF::sdf PROPERTIES
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/lib/libsdf.so"
  IMPORTED_SONAME_RELEASE "libsdf.so"
  )

list(APPEND _cmake_import_check_targets SDF::sdf )
list(APPEND _cmake_import_check_files_for_SDF::sdf "${_IMPORT_PREFIX}/lib/libsdf.so" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
