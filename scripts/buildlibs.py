#!/usr/bin/env python3
from scriptutils import *


def build_c_file(file):
    path = Path(file)
    bitcode_path = path.with_suffix(".bc")
    run_cmd("clang", "-c", "-O3", "-emit-llvm", path, "-o", bitcode_path)
    object_path = path.with_suffix(".o")
    run_cmd("llc", bitcode_path, "-o", object_path, "-filetype=obj", "-O=0")


def build_libs():
    with chdir("libs"):
        run_cmd("clang", "-c", "-O3", "-emit-llvm", "builtins.c", "-o", "globalfreebuiltins.bc")

        run_cmd("llc", "-filetype=obj", "-O=0", "globalfreebuiltins.bc", "-o", "globalfreebuiltins.o")
        run_cmd("clang", "-O0", "-c", "globals.c", "-o", "notPICglobals.o")
        run_cmd("ld", "-r", "globalfreebuiltins.o", "notPICglobals.o", "-o", "builtins.o")

        run_cmd("clang", "-c", "-emit-llvm", "-fPIC", "globals.c", "-o", "PICglobals.bc")
        run_cmd("llvm-link", "globalfreebuiltins.bc", "PICglobals.bc", "-o", "builtins.bc")

        run_cmd("rm", "globalfreebuiltins.bc", "globalfreebuiltins.o", "PICglobals.bc", "notPICglobals.o")

        build_c_file("fs.c")
        build_c_file("math_.c")
        build_c_file("packing.c")

    print("Libraries built")


if __name__ == "__main__":
    cd_to_proj_root()
    build_libs()
