# Epsilon

Epsilon is an **in development**, compiled, statically typed, semi-memory safe programming language with automatic refrence counting. It compiles through LLVM, meaning it can be ran on almost any system.

# Dependencies

Epsilon depends on:

* Python (3.10 or greater)
    * The `virtualenv` package
* Mono 5
* LLVM 14
* libstdc++

# Setup

To setup and build Epsilon, run:

    ./setup.bash
    ./build.bash

# Running Code

*Compilation of Epsilon code to machine code is not yet implemented.*
The file extention of an Epsilon file should be .ep, .eps, .epsilon, or .ε. To compile an Epsilon file, run the command:

    mono Epsilon.exe compile <input file path> <output file path>
