#!/usr/bin/env python3
from scriptutils import *
from buildcs import build_cs
from buildlibs import build_libs
from bootstrap import bootstrap


def setup():
    print("Installing epslc...")

    print("Loading submodules...")
    run_cmd("git", "submodule", "init")
    run_cmd("git", "submodule", "update", "--recursive", "--remote")
    print("Loaded submodules")

    print("Building executable...")
    build_cs()
    print("Executable built")

    print("Setting up libraries...")
    build_libs()

    print("Bootstrapping backend...")
    bootstrap()

    print("Completing setup...")
    run_cmd("ln", "-fs", "../../scripts/pre-commit.py", ".git/hooks/pre-commit")
    run_cmd("mkdir", "-p", "temp")

    print()
    print("Setup complete")
    print("Run `sudo ./scripts/install.py` to install epslc")


if __name__ == "__main__":
    cd_to_proj_root()
    setup()
