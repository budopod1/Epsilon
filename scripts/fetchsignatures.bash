#!/usr/bin/env bash
set -e

cd "${0%/*}/../temp"
../Compilers/signatures "$@"
