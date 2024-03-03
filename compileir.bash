#!/usr/bin/env bash
set -e
cd "${0%/*}"

opt code-linked.bc -O3 -o code-opt.bc
clang code-opt.bc -o code -lc -lm
