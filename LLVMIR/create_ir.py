import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path
from common import *
from structs import Struct, Array
from functions import Function
from program import Program
from extern_funcs import EXTERN_FUNCS
from builtins_ import BUILTINS
import re


epsilon_folder = Path.cwd()


def create_ir(data):
    module = ir.Module(name="main")

    with open(epsilon_folder / "builtins.ll") as builtins_file:
        builtins_text = builtins_file.read()

    module.data_layout = re.search("target datalayout ?= ?\"([:\\-a-zA-Z0-9]+)\"", builtins_text).group(1)

    module.triple = llvm.get_default_triple()

    program = Program(module, data["path"])

    for struct in data["structs"]:
        program.add_struct(Struct(
            program, struct["id"], struct["name"], struct["fields"]
        ))

    for func_name, func_data in EXTERN_FUNCS.items():
        program.add_extern_func(func_name, func_data)

    program.builtins = BUILTINS

    program.add_module_functions(data["module_functions"])

    functions = []
    for function_data in data["functions"]:
        function = Function(
            program, function_data["id"], function_data
        )
        program.add_function(function)
        functions.append(function)

    for function in program.functions.values():
        function.compile_ir()

    return module


def main(*, print_ir=False):
    global epsilon_folder
    
    epsilon_folder = Path(__file__).parent.parent
    with open(epsilon_folder / "code.json") as file:
        data = orjson.loads(file.read())

    module = create_ir(data)

    if print_ir:
        print(module)

    with open(epsilon_folder / "code.ll", "w") as file:
        file.write(str(module))


if __name__ == "__main__":
    main()
