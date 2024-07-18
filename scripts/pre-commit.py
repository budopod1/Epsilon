#!/usr/bin/env python3
from pathlib import Path
import subprocess
import sys

root = Path(sys.argv[0]).resolve().parent.parent

changed_proc = subprocess.run(
    ["git", "diff", "--name-only", "--cached"], capture_output=True, text=True
)
changed_files = filter(bool, changed_proc.stdout.split("\n"))

fixed_files = []

for path in changed_files:
    file = root/path

    if file.suffix not in [".cs", ".epsl", ".py", ".c", ".bash"]:
        continue

    og_text = file.read_text()
    new_text = "\n".join(line.rstrip(" ") for line in og_text.split("\n"))
    file.write_text(new_text)
    if new_text != og_text:
        print("fixing whitespace for", path)
        fixed_files.append(file)

if fixed_files:
    subprocess.run(
        ["git", "add", *fixed_files]
    )
