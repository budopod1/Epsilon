import llvmlite.binding as llvm
from llvmlite import ir
import json
import subprocess
from ctypes import CFUNCTYPE, c_double, c_float, c_int, c_uint, c_ushort, c_ulong, c_long
import re
from pathlib import Path
import stat


def compile_ir(module):
    llvm.initialize()
    llvm.initialize_native_target()
    llvm.initialize_native_asmprinter()

    target = llvm.Target.from_default_triple()
    target_machine = target.create_target_machine()

    backing_mod = llvm.parse_assembly("")
    engine = llvm.create_mcjit_compiler(backing_mod, target_machine)

    module = llvm.parse_assembly(str(module))
    module.verify()
    engine.add_module(module)
    engine.finalize_object()
    engine.run_static_constructors()

    return engine


def main():
    print("Running...")

    with open("code-opt.ll") as file:
        module = file.read()

    engine = compile_ir(module)

    main_ptr = engine.get_function_address("main")
    main = CFUNCTYPE(c_int)(main_ptr)

    print(main())


if __name__ == "__main__":
    main()
