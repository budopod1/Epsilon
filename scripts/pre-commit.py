#!/usr/bin/env python3
from pathlib import Path
import subprocess
import sys

CHECK_SUBTREE_FILES = False
CHECK_REPO_FILES = False

root = Path(sys.argv[0]).resolve().parent.parent

if CHECK_SUBTREE_FILES:
    list_changed_command = ["find"]
elif CHECK_REPO_FILES:
    list_changed_command = ["git", "ls-tree", "-r", "HEAD", "--name-only"]
else:
    list_changed_command = ["git", "diff", "--name-only", "--cached"]

list_proccess = subprocess.run(
    list_changed_command, capture_output=True, text=True
)
check_files = filter(bool, list_proccess.stdout.split("\n"))

fixed_files = []

for path in check_files:
    file = root/path

    if file.suffix not in [".cs", ".epsl", ".py", ".c", ".h", ".cpp", ".bash"]:
        continue

    if not file.exists():
        continue

    if file.is_symlink():
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

if fixed_files and not CHECK_SUBTREE_FILES:
    subprocess.run(
        ["git", "add", *fixed_files]
    )
