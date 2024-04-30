#!/usr/bin/env bash
set -e
cd "${0%/*}"

find . -name "*.cs" \! -name "Epsilon.cs" -print0 | xargs -0 csc -debug Epsilon.cs
