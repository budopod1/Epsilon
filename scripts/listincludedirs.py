#!/usr/bin/env python3
from scriptutils import *
from mapLLVMcmd import run_LLVM_cmd


def list_include_dirs(language):
    if is_windows():
        process_out = run_LLVM_cmd("clang-cpp", "-v", "-x", language, "NUL",
            capture_out=True)
    elif is_macos():
        process_out = run_LLVM_cmd("clang-cpp", "-v", "-x", language, "/dev/null",
            capture_out=True)
    else:
        process_out = run_cmd("cpp", "-v", "-x", language, "/dev/null",
            capture_out=True)

    is_search_list = False
    for line in process_out.split("\n"): # type: ignore
        if is_search_list:
            if line == "End of search list.":
                is_search_list = False
            elif line.startswith(" "):
                yield line[1:]
        elif line == "#include <...> search starts here:":
            is_search_list = True


if __name__ == "__main__":
    print("\n".join(list_include_dirs(sys.argv[1])))
