#!/usr/bin/env bash
set -e
mono App.exe compile $1 code
llvm-dis -o code-opt.ll code.bc
source venv/bin/activate
python testirfunc.py
