#!/usr/bin/env bash
set -e
llvm-link code.ll builtins.bc -o code-linked.bc
opt -O3 code-linked.bc -o code-opt.bc
