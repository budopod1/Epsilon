import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path
from common import *


class Array:
    def __init__(self, program, id_, type_):
        self.program = program
        self.id_ = id_
        self.type_ = type_
        self.generic = type_["generics"][0]
        field_ir_types = [
            REF_COUNTER_FIELD, 
            ir.IntType(64), # capacity
            ir.IntType(64), # element count
            ir.PointerType(make_type_(program, self.generic))
        ]
        self.ir_type = ir.LiteralStructType(field_ir_types).as_pointer()
        ir.global_context.get_identified_type(
            "a"+str(id_)
        ).set_body(*field_ir_types)


class Struct:
    def __init__(self, program, name, fields):
        self.program = program
        self.name = name
        self.fields = fields
        field_ir_types = [REF_COUNTER_FIELD] + [
            make_type_(program, field["type_"])
            for field in fields
        ]
        self.ir_type = ir.LiteralStructType(field_ir_types).as_pointer()
        ir.global_context.get_identified_type(
            "___"+name
        ).set_body(*field_ir_types)

    def get_index_of_member(self, member):
        for i, field in enumerate(self.fields):
            if field["name"] == member:
                return i
        raise KeyError(f"Struct {name} has no member {member}")

    def get_type__by_index(self, index):
        return self.fields[index]["type_"]

    def get_field_types_(self):
        return (field["type_"] for field in self.fields)
