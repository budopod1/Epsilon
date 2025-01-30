#!/usr/bin/env python3
from scriptutils import *
from mapLLVMcmd import run_LLVM_cmd


def bootstrap():
    run_LLVM_cmd("clang", *(["--rtlib=compiler-rt"]*is_windows()),
        "LLVMIRBuilder/bootstrap.bc", "libs/builtins.bc", "libs/fs.bc",
        "libs/packing.bc", "-o", "LLVMIRBuilder/result", "-Wno-override-module")

    print("Bootstrapped backend")


if __name__ == "__main__":
    cd_to_proj_root()
    bootstrap()
