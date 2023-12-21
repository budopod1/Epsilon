#!/usr/bin/env bash
set -e
llvm-as code.ll -o code.bc
llvm-link code.bc builtins.bc -o code-linked.bc
opt -O3 code-linked.bc -o code-opt.bc
