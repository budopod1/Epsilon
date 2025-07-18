#!/usr/bin/env python3
from scriptutils import *
from epslc import epslc


def build_backend(args=None):
    if args is None:
        args = []
    PROJECTS = [
        "EPSL-Console-Styles/", "EPSL-JSON/", "EPSL-IR-Gen/", "EEWriter/",
        "LLVMIRBuilder/"
    ]
    for proj in PROJECTS:
        if epslc(["compile", proj, *args]) != 0:
            return
    print("Compiled backend")


if __name__ == "__main__":
    cd_to_proj_root()
    build_backend(sys.argv[1:])
