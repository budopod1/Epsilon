# Epsilon

Epsilon is a compiled, statically typed, semi-memory safe programming language with automatic reference counting. It compiles through LLVM, meaning that Epsilon uses the same optimizer as Swift, Clang, and Rust. Epsilon supports Linux, MacOS and Windows.

## Setup

To setup Epsilon, run:

    ./scripts/setup.py

## Dependencies

Epsilon depends on:

* Python 3.10 or above
* .NET 8
* Clang and LLVM 16 or above

On Windows, installation of a C standard library may also be required. MSVC or MigGW are recommended for this.

## epslc

`epslc` is the Epsilon compiler. Use `epslc init` to create a new Epsilon project, and then run `epslc compile` to compile it. More command usage information is available with `epslc --help` and `epslc compile --help`.
