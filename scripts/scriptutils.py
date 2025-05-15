import sys

if sys.version_info < (3, 10):
    sys.exit("Python 3.10 or higher is required")

from pathlib import Path
import subprocess
import os
import urllib.request
import urllib.error
import shutil

try:
    from contextlib import chdir # type: ignore
except ImportError:
    from contextlib import contextmanager

    @contextmanager
    def chdir(temp_cwd):
        old_cwd = os.getcwd()
        os.chdir(temp_cwd)
        try:
            yield
        finally:
            os.chdir(old_cwd)


LOCAL_LLVM_INSTALL_DIR = "LLVM"


def _show_cmd_args(*args):
    if "-x" in sys.argv:
        print(" ".join(repr(arg) if " " in arg else arg for arg in args))


def run_cmd(*args, passthrough=True, capture_out=False, use_dash_x_flag=True):
    args = list(map(str, args))
    if use_dash_x_flag:
        _show_cmd_args(*args)
    try:
        process = subprocess.run(args, capture_output=True)
    except FileNotFoundError:
        abort(f"Cannot find command or executable '{args[0]}'")
    proc_out = process.stdout.decode() + process.stderr.decode()
    if is_windows():
        proc_out = proc_out.replace("\r\n", "\n")
    if process.returncode != 0:
        print(proc_out, end="")
        sys.exit(process.returncode)
    if passthrough:
        print(proc_out.strip() and proc_out, end="")
    if capture_out:
        return proc_out


def has_cmd(cmd) -> bool:
    check_command = ["where" if is_windows() else "which", cmd]
    return subprocess.run(check_command, capture_output=True).returncode == 0


def get_user_shell_name() -> str | None:
    if "SHELL" in os.environ:
        return Path(os.environ["SHELL"]).parts[-1]
    return None


def run_powershell_script(script_path: Path, *args, as_admin=False):
    if as_admin:
        run_powershell_script(
            Path("scripts") / "runasadmin.ps1", "-ExecutionPolicy", "bypass",
            script_path.resolve(), *args
        )
    else:
        run_cmd("powershell", "-ExecutionPolicy", "bypass", script_path.resolve(), *args)


def add_to_windows_path(dir: Path, ctx: str):
    assert ctx in ["Machine", "User"]
    run_powershell_script(
        Path("scripts") / "addtoPATH.ps1",
        dir.resolve(), ctx, as_admin=ctx=="Machine"
    )


def get_project_root() -> Path:
    return Path(__file__).parents[1].absolute()


def cd_to_proj_root():
    os.chdir(get_project_root())


def is_windows() -> bool:
    return sys.platform.startswith("win")


def is_macos() -> bool:
    return sys.platform == "darwin"


def is_root() -> bool:
    return os.geteuid() == 0 # type: ignore


def abort(msg: str | None=None):
    if msg is not None:
        print(msg, file=sys.stderr)
    sys.exit(1)


def cmd_options_prompt(*options: str) -> int:
    while True:
        for i, option in enumerate(options):
            print(f"[{i+1}] {option}")
        opt_num_str = input("#? ")
        try:
            opt_num = int(opt_num_str)
            if 1 <= opt_num <= len(options):
                return opt_num-1
            else:
                print(f"Please select a valid option number.")
        except ValueError:
            print("Please enter a number.")


def format_file_size(size: int):
    if size < 1_000:
        suffix = " bytes"
        scale = 1
    elif size < 1_000_000:
        suffix = "kB"
        scale = 1_000
    elif size < 1_000_000_000:
        suffix = "MB"
        scale = 1_000_000
    else:
        suffix = "GB"
        scale = 1_000_000_000
    return f"{size / scale:.0f}{suffix}"


def _download_file(remote_url, response, file):
    print(f"Downloading {remote_url}", end="")
    file_size = int(response.headers["Content-Length"])
    if file_size is not None:
        print(f" ({format_file_size(file_size)})", end="")
    print("...")
    total_read_amount = 0
    blocksize = 8192
    last_percent = None
    while True:
        percent = round(100 * total_read_amount / file_size)
        if file_size is not None and percent != last_percent:
            print(f"\r{percent:02d}%", end="")
            last_percent = percent
        read_data = response.read(blocksize)
        if not read_data:
            break
        file.write(read_data)
        total_read_amount += len(read_data)
    print("\rDownload complete")


def download_file(remote_url, local_path):
    try:
        with open(local_path, "wb") as file:
            with urllib.request.urlopen(remote_url) as response:
                _download_file(remote_url, response, file)
    except urllib.error.URLError as e:
        print(f"Network error: {e}")
        abort()


__all__ = [
    "Path", "sys", "os", "chdir", "run_cmd", "has_cmd", "get_user_shell_name",
    "run_powershell_script", "add_to_windows_path", "is_windows", "is_macos",
    "get_project_root", "cd_to_proj_root", "is_root", "abort",
    "cmd_options_prompt", "download_file", "shutil", "LOCAL_LLVM_INSTALL_DIR"
]
