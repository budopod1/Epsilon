from pathlib import Path


FILE_EXTENSIONS = ["py", "nix", "cs", "ε", "ep", "eps", "epsilon", "bash"]
IGNORE_NAMES = []
IGNORE_FOLDERS = ["venv"]


def main():
    files = sum([
        list(Path.cwd().glob("**/*."+FILE_EXTENSION))
        for FILE_EXTENSION in FILE_EXTENSIONS
    ], [])
    lines = 0
    for file in files:
        if file.name in IGNORE_NAMES:
            continue
        if any(folder in file.parts for folder in IGNORE_FOLDERS):
            continue
        txt = file.read_text()
        file_lines = 1 + txt.count("\n")
        print(file, file_lines)
        lines += file_lines
    print("The code has", lines, "lines")


if __name__ == "__main__":
    main()
