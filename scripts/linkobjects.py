#!/usr/bin/env python3
from scriptutils import *
from mapLLVMcmd import run_LLVM_cmd


def link_objects(srcs, dest):
    if is_windows():
        run_LLVM_cmd("lld-link", "/lib", *srcs, "/out:"+dest, "/subsystem:WINDOWS")
    else:
        run_cmd("ld", "-r", *srcs, "-o", dest)


if __name__ == "__main__":
    dest, *srcs = sys.argv[1:]
    link_objects(srcs, dest)
