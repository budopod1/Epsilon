#!/usr/bin/env python3
from scriptutils import *


def build_cs():
    run_cmd("dotnet", "build", "--property:OutputPath=bin")


if __name__ == "__main__":
    cd_to_proj_root()
    build_cs()
