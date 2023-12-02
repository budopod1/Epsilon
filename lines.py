import argparse
from pathlib import Path


FILE_EXTENSIONS = ["py", "nix", "cs", "ε", "ep", "eps", "epsilon", "bash"]
IGNORE = ["venv", ".local"]


def main(*, verbosity, extentions, ignore):
    files = sum([
        list(Path.cwd().glob("**/*."+FILE_EXTENSION))
        for FILE_EXTENSION in extentions
    ], [])
    lines = 0
    for file in files:
        if file.name in ignore:
            if verbosity >= 2:
                print(f"Skipping file {file}, it has an ignored name")
            continue
        if any(folder in file.parts for folder in ignore):
            if verbosity >= 2:
                print(f"Skipping file {file}, it is in an ignored folder")
            continue
        txt = file.read_text()
        file_lines = 1 + txt.count("\n")
        if verbosity >= 1:
            print(file, "has", file_lines, "lines")
        lines += file_lines
    print("The code has", lines, "lines")


def en_list(items):
    if len(items) == 0:
        return "none"
    if len(items) == 1:
        return items[0]
    if len(items) == 2:
        return " and ".join(items)
    return ", ".join(items[:-1]) + ", and " + items[-1]


def cmdline():
    parser = argparse.ArgumentParser(
        prog="Line Counter",
        description=f"Count the lines in this project, in files with extention(s) {en_list(list(map((lambda ext: '.'+ext), FILE_EXTENSIONS)))}, that do not have the name and are not in the folders {en_list(IGNORE)}."
    )
    parser.add_argument("-v", "--verbosity", type=int, default=0, choices=range(0, 3))
    parser.add_argument("-e", "--extensions", help="override file extentions to include", action="extend", type=str, nargs="*")
    parser.add_argument("-i", "--ignore", help="override files and folders to ignore", action="extend", type=str, nargs="*")

    args = parser.parse_args()

    extentions = args.extensions or FILE_EXTENSIONS
    ignore = args.ignore or IGNORE
    main(verbosity=args.verbosity, extentions=extentions, ignore=ignore)


if __name__ == "__main__":
    cmdline()
