#!/usr/bin/env bash
set -e
cd "${0%/*}"

if [ ./Utils/runcommand.c -nt ./Utils/runcommand.so ]; then
    clang ./Utils/runcommand.c -o ./Utils/runcommand.so -shared -Wall -Wextra -Wno-unused-parameter
fi

if [ ./Compilers/signatures.c -nt ./Compilers/signatures ]; then
    clang ./Compilers/signatures.c -o ./Compilers/signatures -lclang -Wall -Wextra -Wno-unused-parameter
fi

dotnet build --property:OutputPath=bin
