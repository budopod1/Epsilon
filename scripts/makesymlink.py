from scriptutils import *


def make_symlink(target, location):
    # if a symlink already exists at the location, delete it
    Path(location).unlink(missing_ok=True)
    try:
        os.symlink(Path(target).absolute(), location)
    except OSError:
        if is_windows():
            # we probably don't have sufficent permissions to create symlinks
            shutil.copy2(target, location)
        else:
            raise


if __name__ == "__main__":
    make_symlink(*sys.argv[1:])
