# Epsilon

Epsilon is an in development, compiled, statically typed, semi-memory safe programming language with automatic refrence counting. It compiles through LLVM, meaning Epsilon code can be ran on almost any system. Currently, however, only linux is supported, although this is likely to change soon.

# Dependencies

Epsilon depends on:

* Python (3.10 or greater)
    * The `virtualenv` package
* Mono 5
* LLVM 14
* libstdc++
* Clang

# Setup

To setup and build Epsilon, run:

    ./setup.bash

# Running Code

The file extention of an Epsilon file should be .ep, .eps, .epsilon, or .ε. To compile an Epsilon file, run the command:

    mono Epsilon.exe compile <input file path> <output file path>

# Building

After changes are made to the C#, the C# must be rebuilt with `./build.bash`. After `builtins.c` is editted, the builtins can be rebuilt with `./buildbuiltins.bash`.

# Testing

Epsilon can be tested with

    ./test.bash
