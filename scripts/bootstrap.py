#!/usr/bin/env python3
from scriptutils import *
from mapLLVMcmd import run_LLVM_cmd


def bootstrap():
    c_files = []
    for dir_name in EPSL_SOURCE_DIRS:
        c_files.extend(Path(dir_name).glob("*.c"))

    run_LLVM_cmd("clang", "--rtlib=compiler-rt", "-Wno-override-module",
        "LLVMIRBuilder/bootstrap.bc", "libs/builtins.bc", "libs/fileio.bc",
        "libs/conversion.bc", *c_files, "-o", "LLVMIRBuilder/result",
        "-I"+str(Path("libs").absolute()),
        *(["-lm"]*(not is_windows())))

    print("Bootstrapped backend")


if __name__ == "__main__":
    cd_to_proj_root()
    bootstrap()
