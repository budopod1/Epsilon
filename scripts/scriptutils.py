from pathlib import Path
import subprocess
import sys
import os
from contextlib import chdir


def run_cmd(*args: str):
    args = list(map(str, args))
    if "-x" in sys.argv:
        print(" ".join(repr(arg) if " " in arg else arg for arg in args))
    return_code = subprocess.run(args).returncode
    if return_code != 0:
        sys.exit(return_code)


def cd_to_proj_root():
    os.chdir(Path(__file__).parents[1].absolute())


def is_root() -> bool:
    return os.geteuid() == 0


def abort(msg: str | None=None):
    if msg is not None:
        print(msg, file=sys.stderr)
    sys.exit(1)


__all__ = ["Path", "sys", "chdir", "run_cmd", "cd_to_proj_root", "is_root", "abort"]
