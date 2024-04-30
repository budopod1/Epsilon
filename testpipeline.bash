#!/usr/bin/env bash
cd "${0%/*}"
mono --debug Epsilon.exe compile $1 -o result
if [ $? == 0 ]; then
    ./result
    echo $?
fi
