import llvmlite.binding as llvm
from llvmlite import ir
import orjson


REF_COUNTER_FIELD = ir.IntType(32)


class Array:
    def __init__(self, program, id_, type_):
        self.program = program
        self.id_ = id_
        self.type_ = type_
        self.generic = type_["generics"][0]
        field_ir_types = [
            REF_COUNTER_FIELD, 
            ir.IntType(32), 
            ir.PointerType(create_type_(program, self.generic))
        ]
        self.ir_type = ir.LiteralStructType(field_ir_types).as_pointer()
        ir.global_context.get_identified_type(
            "___a"+str(id_)
        ).set_body(*field_ir_types)


class Struct:
    def __init__(self, program, name, fields):
        self.program = program
        self.name = name
        self.fields = fields
        field_ir_types = [REF_COUNTER_FIELD] + [
            create_type_(program, field["type_"])
            for field in fields
        ]
        self.ir_type = ir.LiteralStructType(field_ir_types).as_pointer()
        ir.global_context.get_identified_type(
            name
        ).set_body(*field_ir_types)


class Function:
    def __init__(self, program, id_, return_type_, arguments):
        self.program = program
        self.id_ = id_
        self.return_type_= return_type_
        self.arguments = arguments
        self.ir_type = ir.FunctionType(
            create_type_(program, return_type_),
            [
                create_type_(program, argument["type_"]) 
                for argument in arguments
            ]
        )
        self.ir = ir.Function(program.module, self.ir_type,
                              name="___f"+str(id_))


class Program:
    def __init__(self, module):
        self.module = module
        self.functions = {}
        self.structs = {}
        self.array_ids = {}
        self.arrays = {}

    def add_function(self, function):
        self.functions[function.id_] = function

    def add_struct(self, struct):
        self.structs[struct.name] = struct

    def add_array(self, array):
        self.arrays[array.id_] = array


def create_type_(program, data):
    generics = [
        create_type_(program, generic)
        for generic in data["generics"]
    ]

    bits = data["bits"]
    match data["name"], generics:
        case "Q", []:
            return {
                16: ir.HalfType,
                32: ir.FloatType,
                64: ir.DoubleType
            }[bits]()
        case ("W" | "Z" | "Bool" | "Byte") as name, []:
            return ir.IntType(
                {
                    "W": 32,
                    "Z": 32,
                    "Bool": 1,
                    "Byte": 8
                }[name] if bits is None else bits
            )
        case "Void", []:
            return ir.VoidType()
        case "Pointer", [pointee]:
            return pointee.as_pointer()
        case "Array", [sub]:
            id_ = program.array_ids[freeze_json(data)]
            for id_ in program.arrays:
                return program.arrays[id_].ir_type
            else:
                return ir.global_context.get_identified_type(
                    "___a"+str(id_)
                ).as_pointer()
        case name, []:
            if name in program.structs:
                return program.structs[name].ir_type
            else:
                return ir.global_context.get_identified_type(
                    name
                ).as_pointer()
    
    raise TypeError(f"Invalid type {data}")


def traverse(json):
    yield json
    if isinstance(json, list):
        for elem in json:
            yield from traverse(elem)
    elif isinstance(json, dict):
        for elem in json.values():
            yield from traverse(elem)


def freeze_json(json):
    if isinstance(json, list):
        return tuple([
            freeze_json(elem) for elem in json
        ])
    elif isinstance(json, dict):
        return frozenset({
            (key, freeze_json(value))
            for key, value in json.items()
        })
    return json


def create_ir(data):
    module = ir.Module(name="main")
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

    for function in data["functions"]:
        program.add_function(Function(
            program, function["id"], function["return_type_"],
            function["arguments"]
        ))

    return module


def main():
    with open("code.json") as file:
        data = orjson.loads(file.read())

    module = create_ir(data)

    with open("code.ll", "w") as file:
        file.write(str(module))


if __name__ == "__main__":
    main()
