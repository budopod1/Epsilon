#!/usr/bin/env python3
from scriptutils import *


def build_misc():
    run_cmd("clang", "C-Run-Command/runcommand.c", "-o", "C-Run-Command/runcommand.so",
        "-shared", "-Wall", "-Wextra", "-Wno-unused-parameter")

    run_cmd("clang", "Compilers/signatures.c", "-o", "Compilers/signatures", "-lclang",
        "-Wall", "-Wextra", "-Wno-unused-parameter")


if __name__ == "__main__":
    cd_to_proj_root()
    build_misc()
