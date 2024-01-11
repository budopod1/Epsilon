# Epsilon

Epsilon is a compiled, statically typed, semi-memory safe programming language with automatic refrence counting. It compiles through LLVM, meaning that Epsilon uses the same optimizer as Swift, Clang, and Rust. Currently, Epsilon only supports linux.

## Dependencies

Epsilon depends on:

* Python (3.10 or greater)
    * The `virtualenv` package
* Mono 5
* LLVM 14
* Clang

## Setup

To setup and build Epsilon, run:

    ./setup.bash

## Using Epsilon

[A intro to Epsilon.](docs/intro.md)

## Running Code

The file extention of an Epsilon file should be .epsl. To compile an Epsilon file, run the command:

    mono <path to Epsilon.exe> compile <input file path> <output file path>

## Building

After Epsilon has been setup, Epsilon's C# can be rebuilt with `./build.bash`, while Epsilon's builtins can be rebuilt with `./buildbuiltins.bash`.

## Testing

Epsilon can be tested with

    ./test.bash
