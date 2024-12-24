#!/usr/bin/env bash
set -e
cd "${0%/*}/.."

ln -fs "$PWD/scripts/epslc.bash" /usr/bin/epslc
