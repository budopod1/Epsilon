# Epsilon

Epsilon is a compiled, statically typed, semi-memory safe programming language with automatic reference counting. It compiles through LLVM, meaning that Epsilon uses the same optimizer as Swift, Clang, and Rust. Currently, Epsilon only supports Linux.

## Dependencies

Epsilon depends on:

* Python 3.10 or above
* .NET 8 or above
* Clang and LLVM 15 or above

## Setup

To setup Epsilon, run:

    ./setup.bash

## Documentation

Epsilon has [outdated documentation here](/docs/syntax.md).

## epslc

`epslc` is the Epsilon compiler. Use `epslc init` to create a new Epsilon project, and then run `epslc compile` to compile it. More command usage information is avaliable with `epslc --help` and `epslc compile --help`.
