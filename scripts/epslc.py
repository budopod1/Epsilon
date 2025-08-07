#!/usr/bin/env python3
from scriptutils import *
import subprocess


def epslc(args):
    executable_location = (get_project_root() / "executables/Epsilon.dll").absolute()
    return subprocess.run(["dotnet", executable_location, *args]).returncode


if __name__ == "__main__":
    try:
        code = epslc(sys.argv[1:])
    except KeyboardInterrupt:
        code = 130
    sys.exit(code)
