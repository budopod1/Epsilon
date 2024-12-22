#!/usr/bin/env bash
set -e
cd "${0%/*}/.."

dotnet build --property:OutputPath=bin
