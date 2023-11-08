{ pkgs }: {
    deps = [
        pkgs.python39Packages.virtualenv
        pkgs.ack
        pkgs.python310
        pkgs.mono5
    ];
}
