#!/usr/bin/env bash
set -e
cd "${0%/*}"

python tests/test.py
