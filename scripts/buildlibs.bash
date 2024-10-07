#!/usr/bin/env bash
set -e
cd "${0%/*}/.."

for path in ./libs/*.c; do
    file="${path%.*}"
    clang -S -emit-llvm $path -o "${file}.ll"
    opt -O3 "${file}.ll" -o "${file}.bc"
    llc "${file}.bc" -o "${file}.o" -filetype=obj -O=0
    if [ "${file##*/}" != "builtins" ]; then
        rm "${file}.ll"
    fi
done
echo "Libraries built"
