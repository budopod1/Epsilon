#!/usr/bin/env bash
find ./*/ -name "*.cs" -exec csc -debug Epsilon.cs {} +
