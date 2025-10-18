#!/usr/bin/env python3
from scriptutils import *
import re


def main():
    for epsl_path in Path.cwd().glob("**/*.epsl"):
        with chdir(epsl_path.parent):
            run_cmd("git", "checkout", "HEAD", "--", epsl_path)
        txt = epsl_path.read_text()
        txt = re.sub(
            r"\n( *)abort (.*?);\n", r"\n\1abort [\2];\n", txt,
            flags=re.DOTALL
        )
        epsl_path.write_text(txt)
    print("done")


if __name__ == "__main__":
    cd_to_proj_root()
    main()
