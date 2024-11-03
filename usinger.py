from pathlib import Path
import subprocess
import sys

pkg = sys.argv[1]

i = 1
while True:
    print("round", i)
    i += 1

    build_proc = subprocess.run(
        ["./build.bash"], capture_output=True, text=True
    )
    if build_proc.returncode == 0:
        print("finished")
        sys.exit(0)
    err_files = {
        Path(line[:line.index(".cs")+3])
        for line in build_proc.stdout.split("\n")
        if line.startswith("/")
    }

    for file in err_files:
        print("fixing", file)

        text = file.read_text()

        if not text.startswith("using"):
            text = "\n" + text

        text = f"using {pkg};\n" + text

        file.write_text(text)
