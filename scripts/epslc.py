#!/usr/bin/env python3
from scriptutils import *


def epslc(args):
    run_cmd("dotnet", Path("executables/Epsilon.dll").absolute(),
        *args, use_dash_x_flag=False)


if __name__ == "__main__":
    epslc(sys.argv[1:])
