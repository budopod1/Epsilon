#!/usr/bin/env bash
set -e
cd "${0%/*}"

for path in ./libs/*.c; do
    file="${path%.*}"
    clang -S -emit-llvm $path -o "${file}.ll"
    llvm-as "${file}.ll" -o "${file}.bc"
done
echo "Libraries built"
