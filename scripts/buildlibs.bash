#!/usr/bin/env bash
set -e
cd "${0%/*}/../libs"

build_c_file() {
    file="$1"
    extensionless="${file%.*}"
    clang -S -emit-llvm "$file" -o "${extensionless}.ll"
    opt -O3 "${extensionless}.ll" -o "${extensionless}.bc"
    llc "${extensionless}.bc" -o "${extensionless}.o" -filetype=obj -O=0
    if [ "${extensionless##*/}" != "builtins" ]; then
        rm "${extensionless}.ll"
    fi
}

build_c_file "builtins.c"
build_c_file "fs.c"
build_c_file "math_.c"
build_c_file "packing.c"

echo "Libraries built"
