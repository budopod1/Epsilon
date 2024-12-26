#!/usr/bin/env python3
from scriptutils import *


def run_backend():
    run_cmd("./LLVMIR/result")


if __name__ == "__main__":
    cd_to_proj_root()
    run_backend()
