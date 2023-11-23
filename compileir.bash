#!/usr/bin/env bash
set -e
cd "${0%/*}"
opt -O3 -o code.bc code.ll
# llc -o code.o -filetype=obj code.bc
# ld code.o -o code
