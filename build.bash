#!/usr/bin/env bash
set -e
cd "${0%/*}"

if [ ./Utils/runcommand.c -nt ./runcommand.so ]; then
    clang ./Utils/runcommand.c -o ./runcommand.so -shared
fi

if [ ./C-EPSL-Translator/signatures.c -nt ./C-EPSL-Translator/signatures ]; then
    clang ./C-EPSL-Translator/signatures.c -o ./C-EPSL-Translator/signatures -lclang
fi

find . -name "*.cs" \! -name "Epsilon.cs" -print0 | xargs -0 csc -debug Epsilon.cs
