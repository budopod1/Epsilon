#!/usr/bin/env bash
set -e
mono App.exe
llvm-dis -o code-opt.ll code.bc
source venv/bin/activate
python testirfunc.py
