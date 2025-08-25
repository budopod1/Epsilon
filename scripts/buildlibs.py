#!/usr/bin/env python3
from scriptutils import *
from linkobjects import link_objects
from mapLLVMcmd import run_LLVM_cmd


def build_c_file(file):
    path = Path(file)
    bitcode_path = path.with_suffix(".bc")
    run_LLVM_cmd("clang", "-c", "-O3", "-emit-llvm", path, "-o", bitcode_path)
    object_path = path.with_suffix(".o")
    run_LLVM_cmd("clang", "-c", bitcode_path, "-o", object_path, "-O1")


def build_libs():
    with chdir("libs"):
        run_LLVM_cmd("clang", "-c", "-O3", "-emit-llvm", "epsilon.c", "-o", "globalfreeepsilon.bc")

        run_LLVM_cmd("clang", "-c", "globalfreeepsilon.bc", "-o", "globalfreeepsilon.o", "-O1")
        run_LLVM_cmd("clang", "-O0", "-c", "globals.c", "-o", "notPICglobals.o")
        link_objects(("globalfreeepsilon.o", "notPICglobals.o"), "epsilon.o")

        run_LLVM_cmd("clang", "-c", "-emit-llvm", *(["-fPIC"]*(not is_windows())), "globals.c", "-o", "PICglobals.bc")
        run_LLVM_cmd("llvm-link", "globalfreeepsilon.bc", "PICglobals.bc", "-o", "epsilon.bc")

        for file in ("globalfreeepsilon.bc", "globalfreeepsilon.o", "PICglobals.bc", "notPICglobals.o"):
            os.unlink(file)

        build_c_file("fileio.c")
        build_c_file("math_.c")
        build_c_file("conversion.c")
        build_c_file("time_.c")
        build_c_file("dllib.c")
        build_c_file("proc.c")
        build_c_file("main.c")

    print("Libraries built")


if __name__ == "__main__":
    cd_to_proj_root()
    build_libs()
