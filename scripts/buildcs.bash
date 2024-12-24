#!/usr/bin/env bash
set -euo pipefail
cd "${0%/*}/.."

dotnet build --property:OutputPath=bin
