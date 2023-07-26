from pathlib import Path


def combine(files):
    imports = set()
    contents = ""

    for file in files:
        started = False
        for line in file.read_text().split("\n"):
            if line == "":
                continue
            if "{" in line:
                started = True
            if started:
                contents += line + "\n"
            else:
                if "/*" in line:
                    break
                imports.add(line)

    return "\n".join(imports) + "\n" + contents


def main():
    with open("Combined.cs", "w") as file:
        file.write(combine(Path.cwd().glob("*/**/*.cs")))


if __name__ == "__main__":
    main()
