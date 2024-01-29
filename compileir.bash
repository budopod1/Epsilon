#!/usr/bin/env bash
set -e
opt code-linked.bc -o code-opt.bc
clang code-opt.bc -o code -lc -lm
