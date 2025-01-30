#!/usr/bin/env python3
from scriptutils import *
from buildbackend import build_backend
from epslc import epslc


def build_bootstrap():
    build_backend()
    epslc(["compile", "LLVMIRBuilder/", "-t", "llvm-bc", "-o",
        "LLVMIRBuilder/bootstrap.bc", "--no-link-builtins",
        "--no-link-builtin-modules"])
    print("Built bootstrap file")


if __name__ == "__main__":
    cd_to_proj_root()
    build_bootstrap()
