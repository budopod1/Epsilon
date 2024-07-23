#!/usr/bin/env bash
set -e

cd "${0%/*}/../temp"
../C-EPSL-Translator/signatures "$@"
