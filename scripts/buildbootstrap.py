#!/usr/bin/env python3
from scriptutils import *
from buildbackend import build_backend
from mapLLVMcmd import run_LLVM_cmd


def build_bootstrap():
    build_backend(["-H", "always", "-t", "llvm-ll", "-o", dev_null()])

    llvm_files = []
    for dir_name in EPSL_SOURCE_DIRS:
        directory = Path(dir_name)
        for ext in [".ll", ".bc"]:
            llvm_files.extend(directory.glob(".*"+ext))

    bootstrap_dest = Path("LLVMIRBuilder") / "bootstrap.bc"
    run_LLVM_cmd("llvm-link", *llvm_files, "-o", bootstrap_dest)

    print("Built bootstrap file")


if __name__ == "__main__":
    cd_to_proj_root()
    build_bootstrap()
