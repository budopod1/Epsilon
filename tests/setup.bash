#!/usr/bin/env bash
set -e
cd "${0%/*}"

clang Internal/cfile.c -S -emit-llvm -o Internal/cfile.bc
echo "Setup tests"
