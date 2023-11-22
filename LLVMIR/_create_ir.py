import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path
from common import *
from structs import Struct, Array
from functions import Function
from program import Program


def create_ir(data):
    module = ir.Module(name="main")
    
    module.triple = llvm.get_default_triple()
    
    program = Program(module)

    program.array_ids = dict(map(
        lambda pair: (freeze_json(pair[1]), pair[0]), 
        enumerate(data["arrays"])
    ))
    
    for i, array in enumerate(data["arrays"]):
        program.add_array(Array(
            program, i, array
        ))

    for struct in data["structs"]:
        program.add_struct(Struct(
            program, struct["name"], struct["fields"]
        ))

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
    with open("code.json") as file:
        data = orjson.loads(file.read())

    module = create_ir(data)

    if print_ir:
        print(module)

    with open("code.ll", "w") as file:
        file.write(str(module))


if __name__ == "__main__":
    main(print_ir=True)
