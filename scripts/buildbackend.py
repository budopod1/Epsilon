#!/usr/bin/env python3
from scriptutils import *
from epslc import epslc


def build_backend(args=None):
    if args is None:
        args = []
    PROJECTS = [
        "EPSL-BinJSON/binjson", "EPSL-IR-Gen/irgen", "EEWriter/eewriter",
        "LLVMIRBuilder/"
    ]
    for proj in PROJECTS:
        epslc(["compile", proj, *args])
    print("Compiled backend")


if __name__ == "__main__":
    cd_to_proj_root()
    build_backend(sys.argv[1:])
