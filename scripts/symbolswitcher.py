#!/usr/bin/env python3
from scriptutils import *
import re


def convert_line(line: str) -> str:
    line = re.sub(r"(^| |\(|\[)%(\w+)", r"\1$\2", line)
    line = re.sub(r"\$(import|global)", r"%\1", line)
    return line


def main():
    for epsl_path in Path.cwd().glob("**/*.epsl"):
        with chdir(epsl_path.parent):
            run_cmd("git", "checkout", "HEAD", "--", epsl_path)
        txt = epsl_path.read_text()
        txt = "\n".join(map(convert_line, txt.split("\n")))
        epsl_path.write_text(txt)
    print("done")


if __name__ == "__main__":
    cd_to_proj_root()
    main()
