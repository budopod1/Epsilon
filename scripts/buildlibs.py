#!/usr/bin/env python3
from scriptutils import *
from linkobjects import link_objects
from mapLLVMcmd import run_LLVM_cmd


def build_c_file(file):
    path = Path(file)
    bitcode_path = path.with_suffix(".bc")
    run_cmd("clang", "-c", "-O3", "-emit-llvm", path, "-o", bitcode_path)
    object_path = path.with_suffix(".o")
    run_LLVM_cmd("clang", "-c", bitcode_path, "-o", object_path, "-O1")


def build_libs():
    with chdir("libs"):
        run_LLVM_cmd("clang", "-c", "-O3", "-emit-llvm", "builtins.c", "-o", "globalfreebuiltins.bc")

        run_LLVM_cmd("clang", "-c", "globalfreebuiltins.bc", "-o", "globalfreebuiltins.o", "-O1")
        run_LLVM_cmd("clang", "-O0", "-c", "globals.c", "-o", "notPICglobals.o")
        link_objects(("globalfreebuiltins.o", "notPICglobals.o"), "builtins.o")

        run_LLVM_cmd("clang", "-c", "-emit-llvm", *(["-fPIC"]*(not is_windows())), "globals.c", "-o", "PICglobals.bc")
        run_LLVM_cmd("llvm-link", "globalfreebuiltins.bc", "PICglobals.bc", "-o", "builtins.bc")

        for file in ("globalfreebuiltins.bc", "globalfreebuiltins.o", "PICglobals.bc", "notPICglobals.o"):
            os.unlink(file)

        build_c_file("fs.c")
        build_c_file("math_.c")
        build_c_file("packing.c")

    print("Libraries built")


if __name__ == "__main__":
    cd_to_proj_root()
    build_libs()
