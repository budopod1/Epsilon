#!/usr/bin/env python3
from scriptutils import *
from mapLLVMcmd import run_LLVM_cmd


def get_libclang_required_flags():
    libclang_headers = Path("LLVM/include")
    if libclang_headers.is_dir():
        yield f"-I{libclang_headers.absolute()}"
    clang_libraries = Path("LLVM/lib")
    if clang_libraries.is_dir():
        yield f"-L{clang_libraries.absolute()}"
    if is_windows():
        yield "-llibclang"
    else:
        yield "-lclang"


def build_misc():
    runcommand_out = Path("C-Run-Command") / ("runcommand.dll" if is_windows() else "runcommand.so")
    run_LLVM_cmd("clang", "C-Run-Command/runcommand.c", "-o", runcommand_out,
        "-shared", "-Wall", "-Wextra", "-Wno-unused-parameter")

    run_LLVM_cmd("clang", "Compilers/signatures.c", "-o", "Compilers/signatures", "-Wall",
        "-Wextra", "-Wno-unused-parameter", *get_libclang_required_flags())


if __name__ == "__main__":
    cd_to_proj_root()
    build_misc()
