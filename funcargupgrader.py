from pathlib import Path
import subprocess
import sys

def process_template(old):
    new = ""
    indent = 0

    for char in old:
        if indent == 0 and char == "[":
            # this line has already been migrated
            return old
        elif char == "<":
            new += "[" if indent == 0 else "<"
            indent += 1
        elif char == ">":
            indent -= 1
            new += "]" if indent == 0 else ">"
        else:
            new += char

    if indent != 0:
        print("don't understand template")
        print(old)
        sys.exit(1)

    return new

def process_line(line):
    if "#" not in line or line[-1] != "{":
        return line

    before_hashtag, hashtag, after_hashtag = line.partition("#")
    return before_hashtag + hashtag + process_template(after_hashtag)

def main():
    root = Path(sys.argv[0]).resolve().parent

    for file in root.glob("**/*.epsl"):
        text1 = file.read_text()

        text2 = "\n".join(
            process_line(line)
            for line in text1.split("\n")
        )

        if text2 != text1:
            file.write_text(text2)

main()
