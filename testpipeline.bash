#!/usr/bin/env bash
set -e
cd "${0%/*}"
mono --debug Epsilon.exe compile $1 result
./result
