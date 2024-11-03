#!/usr/bin/env python3
from pathlib import Path
import subprocess
import re

replacer = re.compile("(^|\n)(public(?: (?:abstract|static|final))* (?:class|enum|interface))")

name = Path(".").resolve().name.replace("-", "")

changed_proc = subprocess.run(
    ["git", "ls-tree", "-r", "HEAD", "--name-only"], capture_output=True, text=True
)
changed_files = [
    Path(line)
    for line in changed_proc.stdout.split("\n")
    if line
]

for file in changed_files:
    if file.suffix not in [".cs"]:
        continue

    if not file.exists():
        continue

    text1 = file.read_text()

    text2 = text1.replace("abstract public", "public abstract")
    text2 = replacer.sub(f"\\1namespace {name};\n\\2", text2)

    if text2 != text1:
        file.write_text(text2)
