#!/usr/bin/env bash
set -e
cd "${0%/*}/.."

for path in ./libs/*.c; do
    file="${path%.*}"
    clang -S -emit-llvm $path -Xclang -no-opaque-pointers -o "${file}.ll"
    opt -O3 "${file}.ll" -opaque-pointers=0 -o "${file}.bc"
    llc "${file}.bc" -opaque-pointers=0 -o "${file}.o" -filetype=obj
    if [ "${file##*/}" != "builtins" ]; then
        rm "${file}.ll"
    fi
done
echo "Libraries built"
