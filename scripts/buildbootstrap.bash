#!/usr/bin/env bash
set -euo pipefail
cd "${0%/*}/.."

./scripts/buildbackend.bash
epslc compile LLVMIR/ -t llvm-bc -o LLVMIR/bootstrap.bc --no-link-builtins --no-link-builtin-modules
echo "Built bootstrap file"
