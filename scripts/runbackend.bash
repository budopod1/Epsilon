#!/usr/bin/env bash
set -euo pipefail
cd "${0%/*}/.."

./LLVMIR/result "$@"
