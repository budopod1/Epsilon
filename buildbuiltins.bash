#!/usr/bin/env bash
set -e
cd "${0%/*}"
clang builtins.c -S -emit-llvm -o builtins.ll
llvm-as builtins.ll -o builtins.bc
