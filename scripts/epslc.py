#!/usr/bin/env python3
from scriptutils import *
import subprocess


def epslc(args):
    executable_location = (get_project_root() / "executables/Epsilon.dll").absolute()
    return subprocess.run(["dotnet", executable_location, *args]).returncode


if __name__ == "__main__":
    sys.exit(epslc(sys.argv[1:]))
