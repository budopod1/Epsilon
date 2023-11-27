from llvmlite import ir
from common import *


class Program:
    def __init__(self, module):
        self.module = module
        self.functions = {}
        self.structs = {}
        self.array_ids = {}
        self.arrays = {}
        self.externs = {}
        self.extern_funcs = {}

    def add_extern_func(self, name, data):
        self.externs[name] = data
        self.extern_funcs[name] = ir.Function(
            self.module, make_function_type_(
                self, data["return_type_"], data["arguments"]
            ), name=name
        )

    def call_extern(self, builder, name, params, param_types_, result_type_):
        func = self.externs[name]
        converted_params = [
            convert_type_(self, builder, param, param_type_, argument)
            for param, param_type_, argument in zip(params, param_types_, func["arguments"])
        ]
        return convert_type_(
            self, builder, builder.call(self.extern_funcs[name], converted_params),
            func["return_type_"], result_type_
        )

    def malloc(self, builder, ir_type, count=1):
        size_ptr = builder.gep(builder.inttoptr(i64_of(0), ir_type), [i64_of(1)])
        size = builder.ptrtoint(size_ptr, ir.IntType(64))
        if count > 1:
            size = builder.mul(size, i64_of(count))
        location_i8 = builder.call(self.extern_funcs["malloc"], [size])
        return builder.bitcast(location_i8, ir_type)

    def add_function(self, function):
        self.functions[function.id_] = function

    def add_struct(self, struct):
        self.structs[struct.name] = struct

    def add_array(self, array):
        self.arrays[array.id_] = array
