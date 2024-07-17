from llvmlite import ir
from common import *
from stringify import make_stringify_func
from equals import refrence_equals, value_equals_depth_1, value_equals
from misc_helpers import index_of, compare, dedup, optional_array_access, bitshift
from functions import ModuleFunction
import re


class Program:
    def __init__(self, module, path):
        self.module = module
        self.path = path
        self.functions = {}
        self.module_functions = {}
        self.structs = {}
        self.globals = {}
        self.global_declarations = {}
        self.externs = {}
        self.extern_funcs = {}
        self.check_funcs = {}
        self.builtins = {}
        self.stringifiers = {}
        self.global_counter = 0
        self.refrence_eq_funcs = {}
        self.value_eq_d1_funcs = {}
        self.value_eq_funcs = {}
        self.index_of_funcs = {}
        self.dedup_funcs = {}
        self.comparer_funcs = {}
        self.optional_array_access_funcs = {}
        self.bitshift_funcs = {}

    def setup_scope(self, scope):
        self.globals = scope
        for var in scope:
            global_ = ir.GlobalVariable(
                self.module, make_type_(self, var["type_"]),
                f"{self.path}/global{self.global_id()}"
            )
            global_.initializer = ir.Constant(make_type_(self, var["type_"]), None)
            self.global_declarations[var["id"]] = global_

    def add_module_functions(self, funcs):
        for func in funcs:
            self.module_functions[func["id"]] = ModuleFunction(self, func)

    def get_function(self, id):
        return self.functions.get(id) or self.module_functions.get(id)

    def is_builtin(self, id):
        return id.startswith("builtin")

    def call_builtin(self, id, builder, params, param_types_, result_type_):
        builtin = self.builtins[id]
        casted_params = [
            (
               param if target_type_ is None else
               convert_type_(self, builder, param, param_type_, target_type_)
            )
            for param, param_type_, target_type_ in zip(params, param_types_, builtin["params"])
        ]
        try:
            result, type_ = builtin["func"](
                self, builder, casted_params, param_types_
            )
        except TypeError:
            result = builtin["func"](
                self, builder, casted_params, param_types_, result_type_
            )
            type_ = result_type_

        if not builtin.get("result_is_param", False):
            result_in_params = builtin.get("result_in_params", False)
            if result_in_params:
                if not is_value_type_(type_):
                    incr_ref_counter(self, builder, result, type_)
            for param, param_type_ in zip(params, param_types_):
                if not is_value_type_(param_type_):
                    self.check_ref(builder, param, param_type_)
            if result_in_params:
                if not is_value_type_(type_):
                    dumb_decr_ref_counter(self, builder, result, type_)

        if result_type_ is None:
            return
        return convert_type_(self, builder, result, type_, result_type_)

    def call_function(self, builder, func, params, param_types_):
        converted_params = [
            convert_type_(
                self, builder, param, param_type_,
                argument["type_"]
            )
            for param, param_type_, argument in zip(
                params, param_types_, func.arguments
            )
        ]

        result = builder.call(func.ir, converted_params)

        if not func.takes_ownership:
            if func.return_type_ is not None and func.result_in_params:
                if not is_value_type_(func.return_type_):
                    incr_ref_counter(
                        self, builder, result, func.return_type_
                    )
            for param, param_type_ in zip(params, param_types_):
                if not is_value_type_(param_type_):
                    self.check_ref(builder, param, param_type_)
            if func.return_type_ is not None and func.result_in_params:
                if not is_value_type_(func.return_type_):
                    dumb_decr_ref_counter(
                        self, builder, result, func.return_type_
                    )

        return result

    def add_extern_func(self, name, data):
        self.externs[name] = data
        self.extern_funcs[name] = ir.Function(
            self.module, make_function_type_(
                self, data["return_type_"], data["arguments"],
                data.get("vargs", False)
            ), name=data["name"]
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
        if result_type_ is None:
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
        self.structs[struct.id_] = struct

    def make_elem(self, builder, type_):
        ir_type = make_type_(self, type_)
        result = builder.mul(self.sizeof(builder, ir_type), i64_of(4))
        if is_value_type_(type_):
            result = builder.add(result, i64_of(2))
        if is_nullable_type_(type_):
            result = builder.add(result, i64_of(1))
        return result

    def _free_array(self, block, builder, val, type_):
        content_ptr_ptr = builder.gep(val, [i64_of(0), i32_of(3)])
        content_ptr = builder.load(content_ptr_ptr)
        if is_value_type_(type_):
            self.dumb_free(builder, val)
            self.dumb_free(builder, content_ptr)
            builder.ret_void()
        else:
            cont_branch = builder.append_basic_block("cont")
            forward_branch = builder.append_basic_block("forward")
            final_branch = builder.append_basic_block("final")

            length_ptr = builder.gep(val, [i64_of(0), i32_of(2)])
            length = builder.load(length_ptr)
            has_content = builder.icmp_unsigned("!=", length, i64_of(0))
            builder.cbranch(has_content, cont_branch, final_branch)

            cont_builder = ir.IRBuilder(cont_branch)
            i = cont_builder.phi(ir.IntType(64))
            i.add_incoming(i64_of(0), block)
            elem_ptr = cont_builder.gep(content_ptr, [i])
            elem = cont_builder.load(elem_ptr)
            self.decr_ref(cont_builder, elem, type_)
            i_incr = cont_builder.add(i, i64_of(1))
            i.add_incoming(i_incr, forward_branch)
            should_continue = cont_builder.icmp_unsigned("<", i_incr, length)
            cont_builder.cbranch(should_continue, forward_branch, final_branch)

            forward_builder = ir.IRBuilder(forward_branch)
            forward_builder.branch(cont_branch)

            final_builder = ir.IRBuilder(final_branch)
            self.dumb_free(final_builder, val)
            self.dumb_free(final_builder, content_ptr)
            final_builder.ret_void()

    def _free_file(self, block, builder, val):
        self.call_extern(builder, "freeFile", [val], [File], None)
        builder.ret_void()

    def _free_struct(self, block, builder, val, type_):
        struct = self.structs[type_["name"]]
        for i, field_type_ in enumerate(struct.get_field_types_()):
            if not is_value_type_(field_type_):
                elem_ptr = builder.gep(val, [i64_of(0), i32_of(i+1)])
                elem = builder.load(elem_ptr)
                self.decr_ref(builder, elem, field_type_)
        self.dumb_free(builder, val)
        builder.ret_void()

    def make_check_func(self, key, type_):
        assert type_["name"] != "Optional"
        if is_value_type_(type_):
            return
        ir_type = make_type_(self, type_)
        func = ir.Function(
            self.module, ir.FunctionType(
                ir.VoidType(), [ir_type, make_type_(self, W64)]
            ), name=f"{self.path}/check{len(self.check_funcs)}"
        )
        self.check_funcs[key] = func
        func.attributes.add("alwaysinline")
        entry = func.append_basic_block(name="entry")
        builder = ir.IRBuilder(entry)
        val, refs = func.args
        no_refs = builder.icmp_unsigned("==", refs, REF_COUNTER_FIELD(0))
        free_block = func.append_basic_block(name="free")
        exit_block = func.append_basic_block(name="exit")
        builder.cbranch(no_refs, free_block, exit_block)
        fbuilder = ir.IRBuilder(free_block)
        if type_["name"] == "Array":
            self._free_array(free_block, fbuilder, val, type_["generics"][0])
        elif type_["name"] == "File":
            self._free_file(free_block, fbuilder, val)
        else:
            self._free_struct(free_block, fbuilder, val, type_)
        ebuilder = ir.IRBuilder(exit_block)
        ebuilder.ret_void()
        return func

    def check_ref(self, builder, value, type_, refs=None, no_nulls=False):
        if type_ == Null:
            return
        if is_value_type_(type_):
            return
        if is_nullable_type_(type_) and not no_nulls:
            null_ptr = self.nullptr(builder, make_type_(self, type_))
            with builder.if_then(builder.icmp_unsigned("!=", value, null_ptr)):
                self.check_ref(builder, value, type_, refs, no_nulls=True)
            return
        if type_["name"] == "Optional":
            assert no_nulls
            type_ = type_["generics"][0]
        if refs is None:
            refs = builder.load(builder.bitcast(
                value, REF_COUNTER_FIELD.as_pointer()
            ))
        frozen = freeze_json(type_)
        if frozen in self.check_funcs:
            func = self.check_funcs[frozen]
        else:
            func = self.make_check_func(frozen, type_)
        builder.call(func, [value, refs])

    def decr_ref(self, builder, value, type_, no_nulls=False):
        if type_ == Null:
            return
        if is_value_type_(type_):
            return
        if is_nullable_type_(type_) and not no_nulls:
            null_ptr = self.nullptr(builder, make_type_(self, type_))
            isnt_null = builder.icmp_unsigned("!=", value, null_ptr)
            with builder.if_then(isnt_null):
                self.decr_ref(builder, value, type_, no_nulls=True)
            return
        decred = dumb_decr_ref_counter(
            self, builder, value, type_, no_nulls
        )
        self.check_ref(builder, value, type_, decred, no_nulls)

    def stringify(self, builder, value, type_):
        if type_ == String:
            return self.array_copy(builder, value, String)
        frozen = freeze_json(type_)
        if frozen in self.stringifiers:
            func = self.stringifiers[frozen]
        else:
            func = make_stringify_func(self, frozen, type_, len(self.stringifiers)+1)
        return builder.call(func, [value])

    def global_id(self):
        result = self.global_counter
        self.global_counter += 1
        return result

    def string_literal_array(self, builder, value, size=None, unique=False, name=""):
        # the unique argument, if false, means that the pointer returned is not unique
        # nor is it writable, and it needn't be freed. if true the pointer is a unique
        # pointer allocated during this call
        # the size argument is only meaningful if unique is true, and will be the size
        # of the returned buffer. the size argument should never be lower than len(value)
        # if ommited, the len(value) will be used instead
        size = size or (len(value)+1)
        if unique:
            constant_str = self.string_literal_array(builder, value, size, name=name)
            mem = self.malloc(builder, make_type_(self, Byte), size)
            self.call_extern(
                builder, "memcpy", [mem, constant_str, i64_of(len(value)), i1_of(0)],
                [PointerW8, PointerW8, W64, Bool], None
            )
            return mem
        else:
            ir_type = ir.ArrayType(make_type_(self, Byte), len(value))
            constant = ir.Constant(ir_type, bytearray(value, "utf-8"))
            global_var = ir.GlobalVariable(
                self.module, ir_type, f"{self.path}/string{self.global_id()}"
            )
            global_var.global_constant = True
            global_var.unnamed_addr = True
            global_var.initializer = constant
            return builder.gep(global_var, [i64_of(0), i64_of(0)])

    def refrence_equals(self, builder, type_, v1, v2, invert=False):
        if is_number_type_(type_):
            return compare_values(
                builder, "!=" if invert else "==", v1, v2, type_
            )
        else:
            frozen = (freeze_json(type_), invert)
            if frozen in self.refrence_eq_funcs:
                func = self.refrence_eq_funcs[frozen]
            else:
                self.refrence_eq_funcs[frozen] = None
                func = refrence_equals(
                    self, len(self.refrence_eq_funcs), type_, invert
                )
                self.refrence_eq_funcs[frozen] = func
            return builder.call(func, [v1, v2])

    def value_equals_depth_1(self, builder, type_, v1, v2, invert=False):
        if is_number_type_(type_):
            return compare_values(
                builder, "!=" if invert else "==", v1, v2, type_
            )
        else:
            frozen = (freeze_json(type_), invert)
            if frozen in self.value_eq_d1_funcs:
                func = self.value_eq_d1_funcs[frozen]
            else:
                self.value_eq_d1_funcs[frozen] = None
                func = value_equals_depth_1(
                    self, len(self.value_eq_d1_funcs), type_, invert
                )
                self.value_eq_d1_funcs[frozen] = func
            return builder.call(func, [v1, v2])

    def value_equals(self, builder, type_, v1, v2, depth, invert=False):
        if depth == 1:
            return self.value_equals_depth_1(builder, type_, v1, v2, invert)
        else:
            frozen = (freeze_json(type_), invert)
            if frozen in self.value_eq_funcs:
                func = self.value_eq_funcs[frozen]
            else:
                self.value_eq_funcs[frozen] = None
                func = value_equals(
                    self, len(self.value_eq_funcs), type_, depth, invert
                )
                self.value_eq_funcs[frozen] = func
            return builder.call(func, [v1, v2])

    def index_of(self, builder, arr, elem, type_):
        frozen = freeze_json(type_)
        if frozen in self.index_of_funcs:
            func = self.index_of_funcs[frozen]
        else:
            self.index_of_funcs[frozen] = None
            func = index_of(
                self, len(self.index_of_funcs), type_
            )
            self.index_of_funcs[frozen] = func
        return builder.call(func, [arr, elem])

    def dedup(self, builder, arr, type_):
        frozen = freeze_json(type_)
        if frozen in self.dedup_funcs:
            func = self.dedup_funcs[frozen]
        else:
            self.dedup_funcs[frozen] = None
            func = dedup(
                self, len(self.dedup_funcs), type_
            )
            self.dedup_funcs[frozen] = func
        return builder.call(func, [arr])

    def make_comparer_func(self, type_, invert=False):
        frozen = (freeze_json(type_), invert)
        if frozen in self.comparer_funcs:
            return self.comparer_funcs[frozen]
        else:
            self.comparer_funcs[frozen] = None
            func = compare(
                self, len(self.comparer_funcs), type_, invert
            )
            self.comparer_funcs[frozen] = func
            return func

    def sort_array(self, builder, arr, arr_type_, invert=False):
        generic_type_ = arr_type_["generics"][0]
        elem_size = self.sizeof(builder, make_type_(self, generic_type_))
        comparer_func = self.make_comparer_func(generic_type_, invert)
        self.call_extern(
            builder, "sortArray", [arr, elem_size, comparer_func],
            [arr_type_, W64, ComparerType_], None
        )

    def array_copy(self, builder, arr, arr_type_):
        elem_type_ = arr_type_["generics"][0]
        elem = self.make_elem(builder, elem_type_)
        return self.call_extern(
            builder, "clone", [arr, elem],
            [arr_type_, W64], arr_type_
        )

    def optional_array_access(self, builder, arr, arr_type_, elem_type_, idx):
        frozen = (freeze_json(arr_type_), freeze_json(elem_type_))
        if frozen in self.optional_array_access_funcs:
            func = self.optional_array_access_funcs[frozen]
        else:
            func = optional_array_access(
                self, len(self.optional_array_access_funcs), arr_type_, elem_type_
            )
            self.optional_array_access_funcs[frozen] = func
        return builder.call(func, [arr, idx])

    def bitshift(self, builder, a, b, a_type_, b_type_, is_left):
        frozen = (freeze_json(a_type_), freeze_json(b_type_), is_left)
        if frozen in self.bitshift_funcs:
            func = self.bitshift_funcs[frozen]
        else:
            func = bitshift(
                self, len(self.bitshift_funcs), a_type_, b_type_, is_left
            )
            self.bitshift_funcs[frozen] = func
        return builder.call(func, [a, b])

    def bitshift_left(self, builder, a, b, a_type_, b_type_):
        return self.bitshift(builder, a, b, a_type_, b_type_, is_left=True)

    def bitshift_right(self, builder, a, b, a_type_, b_type_):
        return self.bitshift(builder, a, b, a_type_, b_type_, is_left=False)

    def expect(self, builder, val, expected):
        return self.call_extern(builder, "expect.i1", [val, i1_of(expected)], [Bool, Bool], Bool)

    def verify_array_idx(self, builder, arr, i, idx_type_=None):
        if idx_type_ is None:
            idx_type_ = W64
        i = convert_type_(self, builder, i, idx_type_, W64)
        arr_len = builder.load(builder.gep(arr, [i64_of(0), i32_of(2)]))
        out_of_range = builder.icmp_unsigned(">=", i, arr_len)
        out_of_range = self.expect(builder, out_of_range, False)
        with builder.if_then(out_of_range):
            self.call_extern(builder, "arrayIdxFail", [], [], None)
            builder.unreachable()

    def verify_not_null(self, builder, ptr):
        is_null = builder.icmp_unsigned("==", ptr, ir.Constant(ptr.type, None))
        is_null = self.expect(builder, is_null, False)
        with builder.if_then(is_null):
            self.call_extern(builder, "nullValueFail", [], [], None)
            builder.unreachable()
