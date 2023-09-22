from pathlib import Path


def main():
    cs_files = Path.cwd().glob("**/*.cs")
    lines = 0
    for cs_file in cs_files:
        if cs_file.name in ["Combined.cs"]:
            continue
        txt = cs_file.read_text()
        file_lines = 1 + txt.count("\n")
        print(cs_file, file_lines, lines)
        lines += file_lines
    print("The code has", lines, "lines")


if __name__ == "__main__":
    main()
