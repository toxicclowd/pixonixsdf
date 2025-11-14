import os
import sys
import subprocess
from pathlib import Path

from setuptools import setup, Extension
from setuptools.command.build_ext import build_ext

class CMakeExtension(Extension):
    def __init__(self, name):
        super().__init__(name, sources=[])

class CMakeBuild(build_ext):
    def run(self):
        for ext in self.extensions:
            self.build_extension(ext)

    def build_extension(self, ext):
        extdir = Path(self.get_ext_fullpath(ext.name)).parent.absolute()
        
        cmake_args = [
            f'-DCMAKE_LIBRARY_OUTPUT_DIRECTORY={extdir}',
            f'-DPYTHON_EXECUTABLE={sys.executable}',
            '-DCMAKE_BUILD_TYPE=Release',
        ]

        build_args = ['--config', 'Release']
        
        # Build directory
        build_temp = Path(self.build_temp)
        build_temp.mkdir(parents=True, exist_ok=True)

        # Configure
        subprocess.check_call(
            ['cmake', str(Path(__file__).parent)] + cmake_args,
            cwd=build_temp
        )
        
        # Build
        subprocess.check_call(
            ['cmake', '--build', '.'] + build_args,
            cwd=build_temp
        )

setup(
    name='sdf-cpp',
    version='1.0.0',
    author='SDF Contributors',
    description='C++ SDF library with Python bindings',
    long_description=open('../SDF.Cpp/README.md').read() if os.path.exists('../SDF.Cpp/README.md') else '',
    long_description_content_type='text/markdown',
    ext_modules=[CMakeExtension('sdf_cpp')],
    cmdclass={'build_ext': CMakeBuild},
    zip_safe=False,
    python_requires='>=3.7',
    install_requires=[],
    classifiers=[
        'Development Status :: 4 - Beta',
        'Intended Audience :: Developers',
        'Topic :: Scientific/Engineering :: Mathematics',
        'License :: OSI Approved :: MIT License',
        'Programming Language :: Python :: 3',
        'Programming Language :: Python :: 3.7',
        'Programming Language :: Python :: 3.8',
        'Programming Language :: Python :: 3.9',
        'Programming Language :: Python :: 3.10',
        'Programming Language :: Python :: 3.11',
        'Programming Language :: Python :: 3.12',
        'Programming Language :: C++',
    ],
)
