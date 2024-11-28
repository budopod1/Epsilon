# Epsilon

Epsilon is a compiled, statically typed, semi-memory safe programming language with automatic reference counting. It compiles through LLVM, meaning that Epsilon uses the same optimizer as Swift, Clang, and Rust. Currently, Epsilon only supports Linux.

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

Epsilon has documentation avaliable [here](/docs/syntax.md).

## Running Code

Epsilon files must use the extension `.epsl`. To compile them, run:

    mono <path to Epsilon.exe> compile <input file path> [-o <output file path>]

## Building

After Epsilon has been setup, Epsilon's C# can be rebuilt with `./build.bash`, Epsilon's builtins can be rebuilt with `./buildbuiltins.bash`, and Epsilon's standard library can be rebuilt with `./buildlibs.bash`.

## Testing

Epsilon can be tested with

    ./test.bash
