from scriptutils import *


# if this could be called make_quote_on_quote_symlink, it would be
def make_symlink(target, location):
    if Path(location).exists(follow_symlinks=False):
        return
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
