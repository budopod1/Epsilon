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
    chosen_opt = cmd_options_prompt(
        "Add epslc to PATH in .profile (recommended)",
        "Add epslc to PATH somewhere else"
    )

    add_in_script_path = Path.home() / ".profile"
    if chosen_opt == 1:
        print("Where would you like to add epslc to PATH? (e.g. .bashrc)")
        add_in_script_path = Path.home() / input("> ")

    with open(add_in_script_path) as file:
        script_content = file.read()
    path_to_add = Path(BIN_FOLDER).absolute()
    script_content += f"\nexport PATH=$PATH:{path_to_add}\n"
    with open(add_in_script_path, "w") as file:
        file.write(script_content)


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
        symlink_location = input("Symlink location? ")
        make_symlink(EXECUTION_TARGET, Path(symlink_location) / "epslc")


def install():
    if is_windows():
        windows_install()
    else:
        nonwindows_install()


if __name__ == "__main__":
    cd_to_proj_root()
    install()
