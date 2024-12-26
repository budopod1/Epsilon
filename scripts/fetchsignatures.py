#!/usr/bin/env python3
from scriptutils import *


def fetch_signatures(args):
    run_cmd("./Compilers/signatures", *args)


if __name__ == "__main__":
    cd_to_proj_root()
    fetch_signatures(sys.argv[1:])
