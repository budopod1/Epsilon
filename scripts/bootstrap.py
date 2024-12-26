#!/usr/bin/env python3
from scriptutils import *


def bootstrap():
    run_cmd("clang", "LLVMIR/bootstrap.bc", "libs/builtins.bc", "libs/fs.bc",
        "libs/packing.bc", "-lc", "-o", "LLVMIR/result", "-Wno-override-module")

    print("Bootstrapped backend")


if __name__ == "__main__":
    cd_to_proj_root()
    bootstrap()
