#!/usr/bin/env python3
from scriptutils import *
from buildcs import build_cs
from buildlibs import build_libs
from bootstrap import bootstrap
from buildmisc import build_misc
from unzip import unzip
from mapLLVMcmd import has_LLVM_cmd
from makesymlink import make_symlink


WINDOWS_LLVM_SOURCE = "https://github.com/llvm/llvm-project/releases/download/llvmorg-19.1.6/clang+llvm-19.1.6-x86_64-pc-windows-msvc.tar.xz"
LLVM_ARCHIVE_INNER_FOLDER = "clang+llvm-19.1.6-x86_64-pc-windows-msvc"


def install_llvm_from_URL(url):
    local_archive = Path("temp") / "complete-llvm-windows.tar.xz"
    download_file(url, local_archive)
    install_llvm_from_archive(local_archive, delete_archive=True)


def install_llvm_from_archive(source, delete_archive=False):
    print("Unzipping...")
    unzip(source, ".")
    print("Unzipped")

    if delete_archive:
        os.unlink(source)
    final_dir = Path(LOCAL_LLVM_INSTALL_DIR)

    if final_dir.exists():
        print("There is already a folder for storing the final LLVM installation")
        chosen_opt = cmd_options_prompt(
            "Delete the pre-existing LLVM file or folder",
            "Cancel the installation"
        )
        if chosen_opt == 0:
            if final_dir.is_dir():
                shutil.rmtree(final_dir)
            else:
                final_dir.unlink()
        else:
            abort()

    os.rename(LLVM_ARCHIVE_INNER_FOLDER, final_dir)

    install_llvm_from_folder(final_dir)


def install_llvm_from_folder(final_dir):
    print("Do you want LLVM on your system PATH?")
    chosen_opt = cmd_options_prompt(
        "Add to system PATH",
        "Add to user PATH",
        "Do not add to PATH"
    )
    if chosen_opt != 2:
        if chosen_opt == 0:
            ctx = "Machine"
        elif chosen_opt == 1:
            ctx = "User"
        add_to_windows_path(final_dir / "bin", ctx)
        print("LLVM has been added to your PATH.")

    abort("Please rerun setup.py to continue setup")


def auto_windows_LLVM_installation():
    chosen_opt = cmd_options_prompt("Install from "+WINDOWS_LLVM_SOURCE, "Provide a different URL", "Provide a local copy")
    if chosen_opt == 2:
        source_archive = input("Source .tar.xz file? ")
        install_llvm_from_archive(source_archive)
    else:
        source = WINDOWS_LLVM_SOURCE
        if chosen_opt == 1:
            source = input("Source URL? ")
        install_llvm_from_URL(source)


def assist_windows_LLVM_installation():
    chosen_opt = cmd_options_prompt("Attempt auto-installation of LLVM", "Manually install LLVM")
    if chosen_opt == 0:
        auto_windows_LLVM_installation()
    else:
        print("A complete LLVM installation for Windows can be found at:")
        print(WINDOWS_LLVM_SOURCE)
        abort()


def assist_non_windows_installation():
    print("Please install clang and llvm-dev")
    if has_cmd("brew"):
        print("(e.g. brew install llvm)")
    elif has_cmd("apt"):
        print("(e.g. sudo apt install clang llvm-dev)")


def assist_llvm_installation(has_base_clang):
    if has_base_clang:
        print("Although Clang itself is installed, some additional LLVM tools aren't yet")
    if is_windows():
        assist_windows_LLVM_installation()
    else:
        assist_non_windows_installation()


def require_folder(name):
    dir_path = Path(name)
    if dir_path.is_dir():
        return
    elif dir_path.exists():
        abort(f"{name} must be a folder, not a file")
    dir_path.mkdir()


def verify_dependencies():
    if not has_cmd("git"):
        print("""
git not found.
* try restarting your computer
* try invoking the script through git-bash or cygwin
* add git manually to your PATH
""".strip())
        abort()
    if not has_cmd("dotnet"):
        print("dotnet not found")
        abort()
    if not has_LLVM_cmd("clang"):
        print("clang not found")
        assist_llvm_installation(has_base_clang=False)
        abort()
    if not has_LLVM_cmd("llvm-link"):
        print("Required LLVM tool (llvm-link) not found")
        assist_llvm_installation(has_base_clang=True)
        abort()


def load_submodules():
    run_cmd("git", "submodule", "init")
    run_cmd("git", "submodule", "update", "--recursive", "--remote")
    print("Loaded submodules")


def make_project_symlinks():
    if not is_windows():
        make_symlink("scripts/pre-commit.py", ".git/hooks/pre-commit")
    if is_windows():
        make_symlink("executables/Epsilon.exe", "bin/epslc.exe")
    else:
        make_symlink("scripts/epslc.py", "bin/epslc")
    make_symlink("C-Run-Command/runcommand.c", "libs/runcommand.c")
    make_symlink("C-Run-Command/runcommand.h", "libs/runcommand.h")


def setup():
    print("Setting up epslc...")

    require_folder("temp")
    require_folder("bin")
    require_folder("packages")

    verify_dependencies()

    print("Loading submodules...")
    load_submodules()

    print("Building executable...")
    build_cs()
    print("Executable built")

    print("Setting up libraries...")
    build_libs()

    print("Bootstrapping backend...")
    bootstrap()

    print("Completing setup...")
    build_misc()
    make_project_symlinks()

    print()
    print("Setup complete")
    print("Run ./scripts/install.py to install epslc")


if __name__ == "__main__":
    cd_to_proj_root()
    setup()
