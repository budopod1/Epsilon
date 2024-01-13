#!/usr/bin/env bash
set -e
clang code-opt.bc -o code -lc -lm
