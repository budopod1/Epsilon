#!/usr/bin/env python3
from scriptutils import *


def epslc(args):
    run_cmd("dotnet", "bin/EpsilonLang.dll", *args)


if __name__ == "__main__":
    cd_to_proj_root()
    epslc(sys.argv[1:])
