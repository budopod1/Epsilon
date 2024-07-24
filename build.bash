#!/usr/bin/env bash
set -e
cd "${0%/*}"

if [ ./Utils/runcommand.c -nt ./runcommand.so ]; then
    clang ./Utils/runcommand.c -o ./runcommand.so -shared -Wall -Wextra -Wno-unused-parameter
fi

if [ ./Compiler/signatures.c -nt ./Compiler/signatures ]; then
    clang ./Compiler/signatures.c -o ./Compiler/signatures -lclang -Wall -Wextra -Wno-unused-parameter
fi

find . -name "*.cs" \! -name "Epsilon.cs" -print0 | xargs -0 csc -debug Epsilon.cs
