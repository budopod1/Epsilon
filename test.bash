#!/usr/bin/env bash
set -e
cd "${0%/*}"

python3 tests/test.py
