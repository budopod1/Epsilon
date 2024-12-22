#!/usr/bin/env bash
set -e
cd "${0%/*}/../libs"

build_c_file() {
    file="$1"
    extensionless="${file%.*}"
    clang -c -O3 -emit-llvm "$file" -o "${extensionless}.bc"
    llc "${extensionless}.bc" -o "${extensionless}.o" -filetype=obj -O=0
}

clang -c -O3 -emit-llvm builtins.c -o globalfreebuiltins.bc

llc -filetype=obj -O=0 globalfreebuiltins.bc -o globalfreebuiltins.o
clang -O0 -c globals.c -o notPICglobals.o
ld -r globalfreebuiltins.o notPICglobals.o -o builtins.o

clang -c -emit-llvm -fPIC globals.c -o PICglobals.bc
llvm-link globalfreebuiltins.bc PICglobals.bc -o builtins.bc

rm globalfreebuiltins.bc globalfreebuiltins.o PICglobals.bc notPICglobals.o

build_c_file "fs.c"
build_c_file "math_.c"
build_c_file "packing.c"

echo "Libraries built"
