#!/usr/bin/env python3
from pathlib import Path
import subprocess
import sys

CHECK_ALL_FILES = False

root = Path(sys.argv[0]).resolve().parent.parent

if CHECK_ALL_FILES:
    changed_proc = subprocess.run(
        ["git", "ls-tree", "-r", "HEAD", "--name-only"], capture_output=True, text=True
    )
else:
    changed_proc = subprocess.run(
        ["git", "diff", "--name-only", "--cached"], capture_output=True, text=True
    )
changed_files = filter(bool, changed_proc.stdout.split("\n"))

fixed_files = []

for path in changed_files:
    file = root/path

    if file.suffix not in [".cs", ".epsl", ".py", ".c", ".bash"]:
        continue

    text1 = file.read_text()

    if "\t" in text1:
        print("tab character found in", path)
        sys.exit(1)

    text2 = "\n".join(line.rstrip(" ") for line in text1.split("\n"))
    if text2 != text1:
        print("fixed whitespace for", path)

    if text2 and text2[-1] != "\n":
        print("fixed lack of trailing newline in", path)
        text2 += "\n"

    if text2 != text1:
        file.write_text(text2)
        fixed_files.append(file)

if fixed_files:
    subprocess.run(
        ["git", "add", *fixed_files]
    )
