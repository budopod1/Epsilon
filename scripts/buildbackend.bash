#!/usr/bin/env bash
set -e
cd "${0%/*}/.."

epslc compile EPSL-BinJSON/binjson "$@"
epslc compile EPSL-IR-Gen/irgen "$@"
epslc compile EEWriter/eewriter "$@"
epslc compile LLVMIR/ "$@"
echo "Compiled backend"
