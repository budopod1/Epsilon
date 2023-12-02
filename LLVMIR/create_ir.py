import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path
from common import *
from structs import Struct, Array
from functions import Function
from program import Program
from extern_funcs import EXTERN_FUNCS, EXTERN_ARRAYS
from builtins_ import BUILTINS


def create_ir(data):
    module = ir.Module(name="main")
    
    module.triple = llvm.get_default_triple()
    
    program = Program(module)

    all_arrays = data["arrays"] + [
        fill_type_(array)
        for array in EXTERN_ARRAYS
    ]

    program.array_ids = dict(map(
        lambda pair: (freeze_json(pair[1]), pair[0]), 
        enumerate(all_arrays)
    ))
    
    for i, array in enumerate(all_arrays):
        program.add_array(Array(
            program, i, array
        ))

    for struct in data["structs"]:
        program.add_struct(Struct(
            program, struct["name"], struct["fields"]
        ))

    for func_name, func_data in EXTERN_FUNCS.items():
        program.add_extern_func(func_name, func_data)

    program.builtins = BUILTINS

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
