#!/usr/bin/env python3
from scriptutils import *


def install():
    if not is_root():
        abort("Root permissions are required to install epslc")
    run_cmd("ln", "-fs", Path("scripts/epslc.py").absolute(), "/usr/local/bin/epslc")


if __name__ == "__main__":
    cd_to_proj_root()
    install()
