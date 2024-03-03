#!/usr/bin/env bash
set -e
cd "${0%/*}"

(
    which bash;
    which python;
    which opt;
    which clang;
    which python;
    which llvm-link;
    which llvm-dis;
) > commands.txt
