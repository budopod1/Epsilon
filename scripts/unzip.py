#!/usr/bin/env python3
from scriptutils import *


def unzip(source, dest):
    if has_cmd("tar"):
        run_cmd("tar", "xf", source, "-C", dest)
    else:
        extensions = "".join(Path(source).suffixes)
        abort(f"Don't have a way to unzip {extensions} files")


if __name__ == "__main__":
    unzip(*sys.argv[1:])
