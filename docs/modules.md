# Epsilon Documentation

## Table of Content

1. [Syntax](/docs/syntax.md)
1. [Modules & CLI](/docs/modules.md)
1. [Module Refrence](/docs/modref.md)

## Modules

### Syntax

Epsilon has a module system that allows programs to import both libraries and other Epsilon files.

Files can be imported with the syntax:

    #import path;

### Resolution

Periods in the path are treated as directory separator characters (`\` on Windows, `/` elsewhere), while two periods in a row (or a period at the start) are interpreted as the parent directory marker (`..`). This means that

    import .foo.bar...baz

would be interpreted as "import the file specified by the path `../foo/bar/../../baz`" (which is equivalent to `../baz`).

The Epsilon compiler then searches for the module in the following order:

| Priority | Base directory            | Name prefix | Extention  |
| -------- | ------------------------- | ----------- | ---------- |
| 1        | current working directory | None        | `epslspec` |
| 2        | `libs` folder             | None        | `epslspec` |
| 3        | current working directory | `.`         | `epslspec` |
| 4        | `libs` folder             | `.`         | `epslspec` |
| 5        | current working directory | None        | `epsl`     |
| 6        | `libs` folder             | None        | `epsl`     |
| 7        | current working directory | `.`         | `epsl`     |
| 8        | `libs` folder             | `.`         | `epsl`     |

### Project Mode & Partial Compilation

The Epsilon compiler can either be in "project mode" or "script mode." When in project mode, partial compilation, which is when only files that have been modified will be recompiled, is enabled.

To perform partial compilation, the Epsilon compiler will generate `.epslspec`, `.bc`, and `.ll` files in the same folder as the source code. However, the generated files will be prefixed with a `.`.

Epsilon has script mode enabled by default, but switches to project mode when a project file (`.epslproj`) is found, or if at least two user-written files are used in the compilation. Epsilon automatically generates a `.epslproj` file after completing a compilation in project mode.

Epsilon can be instructed to explicitly use project mode with the `-p` or `--project-mode` flag or to ignore prior results of partial compilation and *not* use project mode (use script mode) with the `-P` or `--no-project-mode` flag.

### Module Semantics

When a module is imported, all functions and structs defined in that module become available for use in your importing file. Those functions and structs will, however, not be available in a third file that imports your file.

## CLI

You can view help on Epsilon's CLI with the `-h`/`--help` flag:

    mono <path to Epsilon.exe> -h

### Compilation

To compile an Epsilon file, simply use the command:

    mono <path to Epsilon.exe> compile <file location>

Optionally, specify the compilation output location, use the `-o`/`--output` flag:

    mono <path to Epsilon.exe> compile <file location> -o <output location>

If no output location is specified, the result will be stored in the same directory as the input file, with the same name, **unless** the input file is named `entry.epsl`, in which case the output will be named `result`.

To write a different file type as output, use the `-t`/`--output-type`:

    mono <path to Epsilon.exe> compile <file location> -t <output location>

The `-p`/`--project-mode` and `-P`/`--no-project-mode` enable and disable project mode respectively.

### Teardown

To remove all partial compilation files and the `.epslproj` file, use:

    mono <path to Epsilon.exe> teardown <.epslproj location>
