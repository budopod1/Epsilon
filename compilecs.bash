#!/usr/bin/env bash
find ./*/ -name "*.cs" -exec csc -debug App.cs {} +
