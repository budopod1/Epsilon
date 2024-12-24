#!/usr/bin/env bash
set -euo pipefail

script_path="$(realpath "$0")"
executable_path="${script_path%/*}/../bin/EpsilonLang.dll"
dotnet "$executable_path" "$@"
