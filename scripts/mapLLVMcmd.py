#!/usr/bin/env python3
from scriptutils import *


LOCAL_LLVM_BIN = get_project_root() / LOCAL_LLVM_INSTALL_DIR / "bin"


def _map_local_LLVM_cmd(LLVM_cmd: str) -> str | None:
    if not LOCAL_LLVM_BIN.is_dir():
        return None
    for file in LOCAL_LLVM_BIN.iterdir():
        if file.stem == LLVM_cmd and file.suffix in ["", ".exe"]:
            return str(file.resolve())
    return None


def _map_global_LLVM_cmd(LLVM_cmd: str) -> str | None:
    for dir in os.environ["PATH"].split(os.path.pathsep):
        for binary in Path(dir).glob(LLVM_cmd+"*"):
            if binary.name == LLVM_cmd:
                return binary.name
            before, _, after = binary.name.rpartition("-")
            if before == LLVM_cmd and all(c.isdigit() for c in after):
                return binary.name


def map_LLVM_cmd(LLVM_cmd: str) -> str:
    return _map_local_LLVM_cmd(LLVM_cmd) or _map_global_LLVM_cmd(LLVM_cmd) or LLVM_cmd


def has_LLVM_cmd(LLVM_cmd: str) -> bool:
    return bool(_map_local_LLVM_cmd(LLVM_cmd) or _map_global_LLVM_cmd(LLVM_cmd))


def run_LLVM_cmd(LLVM_cmd: str, *args, capture_out=False):
    return run_cmd(map_LLVM_cmd(LLVM_cmd), *args, capture_out=capture_out)


__all__ = ["map_LLVM_cmd", "has_LLVM_cmd", "run_LLVM_cmd"]


if __name__ == "__main__":
    run_LLVM_cmd(*sys.argv[1:])
