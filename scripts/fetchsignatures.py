#!/usr/bin/env python3
from scriptutils import *


def fetch_signatures(args):
    print(run_cmd("./Compilers/signatures", *args, capture_out=True), end="")


if __name__ == "__main__":
    cd_to_proj_root()
    fetch_signatures(sys.argv[1:])
