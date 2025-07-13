#!/usr/bin/env python3
from scriptutils import *
from mapLLVMcmd import run_LLVM_cmd


def bootstrap():
    run_LLVM_cmd("clang", "--rtlib=compiler-rt", "-lm", "-Wno-override-module",
        "LLVMIRBuilder/bootstrap.bc", "libs/builtins.bc", "libs/fs.bc",
        "libs/conversion.bc", "-o", "LLVMIRBuilder/result")

    print("Bootstrapped backend")


if __name__ == "__main__":
    cd_to_proj_root()
    bootstrap()
