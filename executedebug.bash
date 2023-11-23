#!/usr/bin/env bash
if [ -f App.exe ]; then
    mono --debug App.exe compile $1 code
fi
