#!/usr/bin/env bash
set -e
cd "${0%/*}"
find ./*/ -name "*.cs" -exec csc Epsilon.cs {} +
