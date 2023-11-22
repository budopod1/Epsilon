#!/usr/bin/env bash
# Compile C#

# required, so we don't run anything if compilation fails
rm -f App.exe

find ./*/ -name "*.cs" -exec csc -debug App.cs {} +
