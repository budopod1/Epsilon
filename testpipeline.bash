#!/usr/bin/env bash
set -e
cd "${0%/*}"
mono --debug Epsilon.exe compile $1 code
llvm-dis -o code-opt.ll code-opt.bc
source venv/bin/activate
python testirfunc.py
