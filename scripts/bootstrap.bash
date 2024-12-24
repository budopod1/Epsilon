#!/usr/bin/env bash
set -euo pipefail
cd "${0%/*}/.."

clang LLVMIR/bootstrap.bc libs/builtins.bc libs/fs.bc libs/packing.bc -lc -o LLVMIR/result -Wno-override-module
echo "Bootstrapped backend"
