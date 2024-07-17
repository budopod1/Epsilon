#!/usr/bin/env bash
set -e

mono "${0%/*}/../Epsilon.exe" "$@"
