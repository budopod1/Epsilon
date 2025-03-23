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
        linux_headers = Path("/usr/lib")
        chosen_version = None
        chosen_headers = None

        for headers in linux_headers.glob("llvm-*/include"):
            llvm_version = int(headers.parts[-2].removeprefix("llvm-"))
            if chosen_version is None or llvm_version > chosen_version:
                chosen_version = llvm_version
                chosen_headers = headers

        if chosen_headers is not None:
            yield f"-I{chosen_headers}"
        if chosen_version is not None:
            yield f"-lclang-{chosen_version}"
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
