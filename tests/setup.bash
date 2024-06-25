#!/usr/bin/env bash
set -e
cd "${0%/*}"

clang Internal/cfile.c -S -emit-llvm -o Internal/cfile.bc
clang Annotations/cfile.c -S -emit-llvm -o Annotations/cfile.bc
echo "Setup tests"
