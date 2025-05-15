#!/usr/bin/env python3
from scriptutils import *
from makesymlink import make_symlink


EXECUTION_TARGET = "scripts/epslc.py"
BIN_FOLDER = "bin"


def windows_install():
    chosen_opt = cmd_options_prompt(
        "Install system-wide",
        "Install for only this user",
        "Cancel installation"
    )

    if chosen_opt != 2:
        if chosen_opt == 0:
            ctx = "Machine"
        elif chosen_opt == 1:
            ctx = "User"
        add_to_windows_path(Path(BIN_FOLDER), ctx)


def install_for_user_nonwindows():
    shell = get_user_shell_name() or "bash"
    rc_file = f".{shell}rc"
    default_script = rc_file if is_macos() else ".profile"

    chosen_opt = cmd_options_prompt(
        f"Add epslc to PATH in {default_script} (recommended)",
        "Add epslc to PATH somewhere else"
    )

    add_in_script_path = Path.home() / default_script
    if chosen_opt == 1:
        where_msg = "Where would you like to add epslc to PATH?"
        if default_script != rc_file:
            where_msg += f" (e.g. {rc_file})"
        print(where_msg)
        add_in_script_path = Path.home() / input("> ")

    path_to_add = Path(BIN_FOLDER).absolute()
    with open(add_in_script_path, "a") as file:
        file.write(f"\nexport PATH=$PATH:{path_to_add}\n")


def install_with_symlink():
    symlink_location = Path(input("Symlink location? "))
    if not symlink_location.is_dir():
        abort("The specified folder was not found")
    make_symlink(EXECUTION_TARGET, symlink_location / "epslc")


def nonwindows_install():
    chosen_opt = cmd_options_prompt(
        "Standard system-wide install (requires root)",
        "Install only for this account",
        "Add a symlink to epslc somewhere",
        "Cancel installation"
    )

    if chosen_opt == 0:
        if not is_root():
            abort("Root permissions are required to install epslc system-wide")
        make_symlink(EXECUTION_TARGET, "/usr/local/bin/epslc")
    elif chosen_opt == 1:
        install_for_user_nonwindows()
    elif chosen_opt == 2:
        install_with_symlink()


def install():
    if is_windows():
        windows_install()
    else:
        nonwindows_install()


if __name__ == "__main__":
    cd_to_proj_root()
    install()
