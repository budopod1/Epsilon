# Epsilon

Epsilon is a compiled, statically typed, semi-memory safe programming language with automatic refrence counting.

# Dependencies

Epsilon depends on:

* Python
    * The `virtualenv` package
* Mono
* LLVM

# Setup

To setup and build Epsilon, run:

    ./setup.bash
    ./build.bash

# Running Code

*Compilation of Epsilon code to machine code is not yet implemented.*
The file extention of an Epsilon file should be .ep, .eps, .epsilon, or .ε. To compile an Epsilon file, run the command:

    mono Epsilon.exe compile <input file path> <output file path>
