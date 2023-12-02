#!/usr/bin/env bash
set -e
cd "${0%/*}"
llvm-as code.ll -o code.bc
llvm-link code.bc builtins.bc -o code-linked.bc
opt -O3 -o code-opt.bc code-linked.bc
# llc -o code.o -filetype=obj code.bc
# ld code.o -o code
