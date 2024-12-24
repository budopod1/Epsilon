#!/usr/bin/env bash
set -euo pipefail
cd "${0%/*}/.."

clang ./C-Run-Command/runcommand.c -o ./C-Run-Command/runcommand.so -shared -Wall -Wextra -Wno-unused-parameter

clang ./Compilers/signatures.c -o ./Compilers/signatures -lclang -Wall -Wextra -Wno-unused-parameter
