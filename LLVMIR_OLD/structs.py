import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path
from common import *


class Struct:
    def __init__(self, program, id_, name, fields, symbol):
        self.program = program
        self.id_ = id_
        self.name = name
        self.fields = fields
        self.symbol = symbol
        self.ir_type = None

    def compile_type_(self):
        field_ir_types = [REF_COUNTER_FIELD] + [
            make_type_(self.program, field["type_"])
            for field in self.fields
        ]
        self.ir_type = ir.LiteralStructType(field_ir_types).as_pointer()
        ir.global_context.get_identified_type(
            self.symbol
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
