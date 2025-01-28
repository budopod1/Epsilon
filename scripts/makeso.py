#!/usr/bin/env python3
from scriptutils import *
from mapLLVMcmd import run_LLVM_cmd


def make_so(output, *args):
    run_LLVM_cmd("clang", "-shared", *(["-fPIC"]*(not is_windows())),
        *args, "-o", output)


if __name__ == "__main__":
    make_so(*sys.argv[1:])
