#!/usr/bin/env bash
set -euo pipefail
cd "${0%/*}/.."

ln -fs "$PWD/scripts/epslc.bash" /usr/local/bin/epslc
