from llvmlite import ir
from common import *
from stringify import make_stringify_func


class Program:
    def __init__(self, module):
        self.module = module
        self.functions = {}
        self.structs = {}
        self.array_ids = {}
        self.arrays = {}
        self.externs = {}
        self.extern_funcs = {}
        self.decr_funcs = {}
        self.builtins = {}
        self.stringifiers = {}
        self.const_counter = 0

    def is_builtin(self, id):
        return id in self.builtins

    def call_builtin(self, id, builder, params, param_types_, result_type_):
        builtin = self.builtins[id]
        casted_params = [
            (
               param if target_type_ is None else 
               convert_type_(self, builder, param, param_type_, target_type_)
            )
            for param, param_type_, target_type_ in zip(params, param_types_, builtin["params"])
        ]
        result, type_ = builtin["func"](
            self, builder, casted_params, param_types_
        )
        if result_type_["name"] == "Void":
            return
        return convert_type_(self, builder, result, type_, result_type_)

    def add_extern_func(self, name, data):
        self.externs[name] = data
        self.extern_funcs[name] = ir.Function(
            self.module, make_function_type_(
                self, fill_type_(data["return_type_"]), [
                    fill_type_(argument)
                    for argument in data["arguments"]
                ], data.get("vargs", False)
            ), name=name
        )

    def call_extern(self, builder, name, params, param_types_, result_type_, vargs=None):
        if vargs is None:
            vargs = []
        func = self.externs[name]
        converted_params = [
            convert_type_(self, builder, param, param_type_, argument)
            for param, param_type_, argument in zip(params, param_types_, func["arguments"])
        ]
        result = builder.call(self.extern_funcs[name], converted_params+vargs)
        if result_type_["name"] == "Void":
            return
        return convert_type_(
            self, builder, result, func["return_type_"], result_type_
        )

    def nullptr(self, builder, ir_type):
        return builder.inttoptr(i64_of(0), ir_type)

    def sizeof(self, builder, ir_type):
        null_ptr = self.nullptr(builder, ir_type.as_pointer())
        size_ptr = builder.gep(null_ptr, [i64_of(1)])
        return builder.ptrtoint(size_ptr, ir.IntType(64))

    def malloc(self, builder, ir_type, count=1, name=""):
        size = self.sizeof(builder, ir_type)
        if count > 1:
            size = builder.mul(size, i64_of(count))
        location_i8 = builder.call(self.extern_funcs["malloc"], [size], name=name)
        return builder.bitcast(location_i8, ir_type.as_pointer())

    def mallocv(self, builder, ir_type, count, name=""):
        size = self.sizeof(builder, ir_type)
        size = builder.mul(size, count)
        location_i8 = builder.call(self.extern_funcs["malloc"], [size], name=name)
        return builder.bitcast(location_i8, ir_type.as_pointer())

    def dumb_free(self, builder, value):
        casted = builder.bitcast(value, ir.IntType(8).as_pointer())
        builder.call(self.extern_funcs["free"], [casted])

    def add_function(self, function):
        self.functions[function.id_] = function

    def add_struct(self, struct):
        self.structs[struct.name] = struct

    def add_array(self, array):
        self.arrays[array.id_] = array

    def make_elem(self, builder, type_):
        ir_type = make_type_(self, type_)
        result = builder.mul(self.sizeof(builder, ir_type), i64_of(2))
        if is_value_type_(type_):
            result = builder.add(result, i64_of(1))
        return result

    def _free_array(self, entry, builder, val, type_):
        content_ptr_ptr = builder.gep(val, [i64_of(0), i32_of(3)])
        content_ptr = builder.load(content_ptr_ptr)
        if is_value_type_(type_):
            self.dumb_free(builder, val)
            self.dumb_free(builder, content_ptr)
            builder.ret_void()
        else:
            cont_branch = builder.append_basic_block("cont")
            final_branch = builder.append_basic_block("final")

            length_ptr = builder.gep(val, [i64_of(0), i32_of(2)])
            length = builder.load(length_ptr)
            has_content = builder.icmp_unsigned("!=", length, i64_of(0))
            builder.cbranch(has_content, cont_branch, final_branch)
    
            cont_builder = ir.IRBuilder(cont_branch)
            i = cont_builder.phi(ir.IntType(64))
            i.add_incoming(i64_of(0), entry)
            elem_ptr = cont_builder.gep(content_ptr, [i])
            elem = cont_builder.load(elem_ptr)
            decr_ref(cont_builder, elem, type_)
            i_incr = cont_builder.add(i, i64_of(1))
            i.add_incoming(i_incr, cont_branch)
            should_continue = cont_builder.icmp_unsigned("<", i_incr, length)
            cont_builder.cbranch(should_continue, cont_branch, final_branch)
            
            final_builder = ir.IRBuilder(final_branch)
            self.dumb_free(final_builder, val)
            self.dumb_free(final_builder, content_ptr)
            final_builder.ret_void()

    def _free_struct(self, entry, builder, val, type_):
        struct = self.structs[self.type_["name"]]
        for i, field_type_ in enumerate(struct.get_field_types_()):
            if not is_value_type(field_type_):
                elem_ptr = builder.gep(val, [i64_of(0), i32_of(i+1)])
                elem = builder.load(elem_ptr)
                decr_ref(builder, elem, field_type_)
        self.dumb_free(builder, val)
        builder.ret_void()

    def make_decr_ref_func(self, type_):
        if is_value_type_(type_):
            return
        ir_type = make_type_(self, type_)
        func = ir.Function(
            self.module, ir.FunctionType(ir.VoidType(), [ir_type]),
            name=f"d{len(self.decr_funcs)}"
        )
        entry = func.append_basic_block(name="entry")
        builder = ir.IRBuilder(entry)
        val, = func.args
        ref_counter = builder.bitcast(val, REF_COUNTER_FIELD.as_pointer())
        decred = builder.sub(builder.load(ref_counter), REF_COUNTER_FIELD(1))
        builder.store(decred, ref_counter)
        no_refs = builder.icmp_unsigned("==", decred, REF_COUNTER_FIELD(0))
        with builder.if_then(no_refs):
            if type_["name"] == "Array":
                self._free_array(entry, builder, val, type_["generics"][0])
            else:
                self._free_struct(entry, builder, val, type_)
        builder.ret_void()
        return func

    def decr_ref(self, builder, value, type_):
        if is_value_type_(type_):
            return
        frozen = freeze_json(type_)
        if frozen in self.decr_funcs:
            func = self.decr_funcs[frozen]
        else:
            func = self.make_decr_ref_func(type_)
            self.decr_funcs[frozen] = func
        builder.call(func, [value])

    def stringify(self, builder, value, type_):
        if type_ == String or type_ == ArrayW8:
            return value
        frozen = freeze_json(type_)
        if frozen in self.stringifiers:
            func = self.stringifiers[frozen]
        else:
            self.stringifiers[frozen] = None
            func = make_stringify_func(self, type_, len(self.stringifiers))
            self.stringifiers[frozen] = func
        return builder.call(func, [value])

    def const_id(self):
        result = self.const_counter
        self.const_counter += 1
        return result

    def string_literal_array(self, builder, value, size=None, unique=False, name=""):
        # the unique argument, if false, means that the pointer returned is not unique
        # nor is it writable, and it needn't be freed. if true the pointer is a unique
        # pointer allocated during this call
        # the size argument is only meaningfulu if unique is true, and will be the size
        # of the returned buffer. the size argument should never be lower than len(value)
        # if ommited, the len(value) will be used instead
        size = size or len(value)
        if unique:
            constant_str = self.string_literal_array(builder, value, size, name=name)
            mem = self.malloc(builder, make_type_(self, Byte), size)
            self.call_extern(
                builder, "memcpy", [mem, constant_str, i64_of(len(value))],
                [PointerW8, PointerW8, W64], VOID
            )
            return mem
        else:
            ir_type = ir.ArrayType(make_type_(self, Byte), len(value))
            constant = ir.Constant(ir_type, bytearray(value, "utf-8"))
            global_var = ir.GlobalVariable(
                self.module, ir_type, f"c{self.const_id()}"
            )
            global_var.global_constant = True
            global_var.unnamed_addr = True
            global_var.initializer = constant
            return builder.gep(global_var, [i64_of(0), i64_of(0)])
        