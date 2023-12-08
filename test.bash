#!/usr/bin/env bash
set -e
cd "${0%/*}"
source venv/bin/activate
python test.py
