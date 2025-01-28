#!/usr/bin/env python3
from scriptutils import *


def epslc(args):
    run_cmd("dotnet", "executables/EpsilonLang.dll", *args,
        use_dash_x_flag=False)


if __name__ == "__main__":
    cd_to_proj_root()
    epslc(sys.argv[1:])
