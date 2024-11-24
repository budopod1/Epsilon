#!/usr/bin/env bash
set -e
cd "${0%/*}/.."

./scripts/buildbackend.bash
epslc compile LLVMIR/ -t llvm-bc -o LLVMIR/bootstrap.bc --no-builtins --no-builtin-modules
echo "Built bootstrap file"
