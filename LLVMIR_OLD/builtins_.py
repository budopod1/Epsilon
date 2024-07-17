from llvmlite import ir
from common import *


def length(program, builder, params, param_types_):
    param, = params
    result_ptr = builder.gep(param, [i64_of(0), i32_of(2)])
    return builder.load(result_ptr), W64


def capacity(program, builder, params, param_types_):
    param, = params
    result_ptr = builder.gep(param, [i64_of(0), i32_of(1)])
    return builder.load(result_ptr), W64


def append(program, builder, params, param_types_):
    array, value = params
    array_type_, value_type_ = param_types_
    length_ptr = builder.gep(array, [i64_of(0), i32_of(2)])
    length = builder.load(length_ptr)
    elem_type_ = array_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, elem_type_))
    program.call_extern(
        builder, "incrementLength", [array, elem_size],
        [ArrayW8, W64], None
    )
    content_ptr = builder.gep(array, [i64_of(0), i32_of(3)])
    content = builder.bitcast(
        builder.load(content_ptr),
        make_type_(program, elem_type_).as_pointer()
    )
    end_ptr = builder.gep(content, [length])
    if not is_value_type_(value_type_):
        incr_ref_counter(program, builder, value, value_type_)
    value_casted = convert_type_(program, builder, value, value_type_, elem_type_)
    builder.store(value_casted, end_ptr)
    return None, None


def require_capacity(program, builder, params, param_types_):
    array, capacity = params
    array_type_, _ = param_types_
    elem_type_ = array_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, elem_type_))
    program.call_extern(
        builder, "requireCapacity", [array, capacity, elem_size],
        [ArrayW8, W64, W64], None
    )
    return None, None


def shrink_mem(program, builder, params, param_types_):
    array, = params
    array_type_, = param_types_
    elem_type_ = array_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, elem_type_))
    program.call_extern(
        builder, "shrinkMem", [array, elem_size], [
            ArrayW8, W64
        ], None
    )
    return None, None


def pop(program, builder, params, param_types_):
    array, idx = params
    array_type_, _ = param_types_
    elem_type_ = array_type_["generics"][0]
    elem_ir_type = make_type_(program, elem_type_)
    elem_size = program.sizeof(builder, elem_ir_type)
    program.verify_array_idx(builder, array, idx)
    content_ptr = builder.load(builder.gep(array, [i64_of(0), i32_of(3)]))
    content_ptr_casted = builder.bitcast(content_ptr, elem_ir_type.as_pointer())
    elem = builder.load(builder.gep(content_ptr_casted, [idx]))
    if not is_value_type_(elem_type_):
        dumb_decr_ref_counter(program, builder, elem, elem_type_)
    program.call_extern(
        builder, "removeAt", [array, idx, elem_size],
        [ArrayW8, W64, W64], None
    )
    return elem, elem_type_


def insert(program, builder, params, param_types_):
    array, idx, value = params
    array_type_, _, value_type_ = param_types_
    elem_type_ = array_type_["generics"][0]
    casted_value = convert_type_(program, builder, value, value_type_, elem_type_)
    elem_ir_type = make_type_(program, elem_type_)
    elem_size = program.sizeof(builder, elem_ir_type)
    program.call_extern(
        builder, "insertSpace", [array, idx, elem_size], [
            ArrayW8, W64,
            W64
        ], None
    )
    content_ptr = builder.load(builder.gep(array, [i64_of(0), i32_of(3)]))
    content_ptr_casted = builder.bitcast(content_ptr, elem_ir_type.as_pointer())
    builder.store(casted_value, builder.gep(content_ptr_casted, [idx]))
    return None, None


def clone(program, builder, params, param_types_):
    array, = params
    array_type_, = param_types_
    return program.array_copy(builder, array, array_type_), array_type_


def extend(program, builder, params, param_types_):
    array1, array2 = params
    array_type_, _ = param_types_ # array types_ will be the same
    elem_type_ = array_type_["generics"][0]
    elem = program.make_elem(builder, elem_type_)
    program.call_extern(
        builder, "extend", [array1, array2, elem], [
            ArrayW8, ArrayW8, W64
        ], None
    )
    return None, None


def concat(program, builder, params, param_types_):
    array1, array2 = params
    array_type_, _ = param_types_ # array types_ will be the same
    elem_type_ = array_type_["generics"][0]
    elem = program.make_elem(builder, elem_type_)
    return program.call_extern(
        builder, "concat", [array1, array2, elem], [
            ArrayW8, ArrayW8, W64
        ], array_type_
    ), array_type_


def unsafe_idx(program, builder, params, param_types_):
    array, idx = params
    array_type_, _ = param_types_
    elem_type_ = array_type_["generics"][0]
    elem_ir_type = make_type_(program, elem_type_)
    content_ptr = builder.load(builder.gep(array, [i64_of(0), i32_of(3)]))
    content_ptr_casted = builder.bitcast(content_ptr, elem_ir_type.as_pointer())
    elem = builder.load(builder.gep(content_ptr_casted, [idx]))
    return elem, elem_type_


def unsafe_int_division(program, builder, params, param_types_, result_type_):
    a, b = (
        convert_type_(program, builder, param, param_type_, result_type_)
        for param, param_type_ in zip(params, param_types_)
    )

    if is_signed_integer_type_(result_type_):
        return builder.sdiv(a, b)
    else:
        return builder.udiv(a, b)


def unsafe_idx_assign(program, builder, params, param_types_):
    array, idx, val = params
    array_type_, _, val_type_ = param_types_
    elem_type_ = array_type_["generics"][0]
    casted_value = convert_type_(program, builder, val, val_type_, elem_type_);
    elem_ir_type = make_type_(program, elem_type_)
    content_ptr = builder.load(builder.gep(array, [i64_of(0), i32_of(3)]))
    content_ptr_casted = builder.bitcast(content_ptr, elem_ir_type.as_pointer())
    builder.store(casted_value, builder.gep(content_ptr_casted, [idx]))
    return None, None


def abs_(program, builder, params, param_types_):
    value, = params
    return program.call_extern(
        builder, "abs", [value, i1_of(0)], [Z32, Bool], W32
    ), W32


def fabs(program, builder, params, param_types_):
    value, = params
    return program.call_extern(builder, "fabs", [value], [Q64], Q64), Q64


def stringify(program, builder, params, param_types_):
    param, = params
    param_type_, = param_types_
    return program.stringify(builder, param, param_type_), String


def print_(program, builder, params, param_types_):
    param, = params
    param_type_, = param_types_
    if param_type_ == String:
        string = param
    else:
        string = program.stringify(builder, param, param_type_)
    program.call_extern(builder, "print", [string], [String], None)
    if param_type_ != String:
        program.dumb_free(builder, builder.load(
            builder.gep(string, [i64_of(0), i32_of(3)])
        ))
        program.dumb_free(builder, string)
    return None, None


def println(program, builder, params, param_types_):
    param, = params
    param_type_, = param_types_
    if param_type_ == String:
        string = param
    else:
        string = program.stringify(builder, param, param_type_)
    program.call_extern(builder, "println", [string], [String], None)
    if param_type_ != String:
        program.dumb_free(builder, builder.load(
            builder.gep(string, [i64_of(0), i32_of(3)])
        ))
        program.dumb_free(builder, string)
    return None, None


def left_pad(program, builder, params, param_types_):
    arr, len, chr = params
    program.call_extern(builder, "leftPad", [arr, len, chr], [String, W64, Byte], None)
    return None, None


def right_pad(program, builder, params, param_types_):
    arr, len, chr = params
    program.call_extern(builder, "rightPad", [arr, len, chr], [String, W64, Byte], None)
    return None, None


def slice_(program, builder, params, param_types_):
    arr, start, end = params
    arr_type_, _, _ = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem = program.make_elem(builder, generic_type_)
    return program.call_extern(
        builder, "slice", [arr, start, end, elem],
        [ArrayW8, W64, W64, W64], ArrayW8
    ), ArrayW8


def countChr(program, builder, params, param_types_):
    str, chr = params
    return program.call_extern(
        builder, "countChr", [str, chr],
        [String, Byte], W64
    ), W64


def count(program, builder, params, param_types_):
    arr, sub = params
    arr_type_, _ = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, generic_type_))
    return program.call_extern(
        builder, "count", [arr, sub, elem_size],
        [ArrayW8, ArrayW8, W64], ArrayW8
    ), ArrayW8


def overlapCount(program, builder, params, param_types_):
    arr, sub = params
    arr_type_, _ = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, generic_type_))
    return program.call_extern(
        builder, "overlapCount", [arr, sub, elem_size],
        [ArrayW8, ArrayW8, W64], ArrayW8
    ), ArrayW8


def nest(program, builder, params, param_types_):
    arr, = params
    arr_type_, = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem = program.make_elem(builder, generic_type_)
    return program.call_extern(
        builder, "nest", [arr, elem], [ArrayW8, W64], ArrayW8
    ), ArrayW8


def split(program, builder, params, param_types_):
    arr, sub = params
    arr_type_, _ = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem = program.make_elem(builder, generic_type_)
    return program.call_extern(
        builder, "split", [arr, sub, elem], [ArrayW8, ArrayW8, W64], ArrayW8
    ), ArrayW8


def starts_with(program, builder, params, param_types_):
    arr, sub = params
    arr_type_, _ = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, generic_type_))
    return program.call_extern(
        builder, "startsWith", [arr, sub, elem_size],
        [ArrayW8, ArrayW8, W64], Bool
    ), Bool


def ends_with(program, builder, params, param_types_):
    arr, sub = params
    arr_type_, _ = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, generic_type_))
    return program.call_extern(
        builder, "endsWith", [arr, sub, elem_size],
        [ArrayW8, ArrayW8, W64], Bool
    ), Bool


def equals(program, builder, params, param_types_):
    v1, v2 = params
    type_1, type_2 = param_types_
    assert type_1 == type_2
    return program.value_equals_depth_1(builder, type_1, v1, v2, invert=False), Bool


def not_equals(program, builder, params, param_types_):
    v1, v2 = params
    type_1, type_2 = param_types_
    assert type_1 == type_2
    return program.value_equals_depth_1(builder, type_1, v1, v2, invert=True), Bool


def equals_depth(program, builder, params, param_types_):
    v1, v2, depth_ir_const = params
    type_1, type_2, _ = param_types_
    assert type_1 == type_2
    depth = depth_ir_const.json_const["value"]
    return program.value_equals(builder, type_1, v1, v2, depth, invert=False), Bool


def not_equals_depth(program, builder, params, param_types_):
    v1, v2, depth_ir_const = params
    type_1, type_2, _ = param_types_
    assert type_1 == type_2
    depth = depth_ir_const.json_const["value"]
    return program.value_equals(builder, type_1, v1, v2, depth, invert=True), Bool


def join(program, builder, params, param_types_):
    arr, sep = params
    type_1, type_2 = param_types_
    generic_type_ = type_2["generics"][0]
    elem = program.make_elem(builder, generic_type_)
    return program.call_extern(
        builder, "join", [arr, sep, elem],
        [type_1, type_2, W64], ArrayW8
    ), ArrayW8


def index_of(program, builder, params, param_types_):
    arr, elem = params
    arr_type_, elem_type_ = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem_casted = convert_type_(
        program, builder, elem, elem_type_, generic_type_
    )
    return program.index_of(builder, arr, elem_casted, generic_type_), Z64


def index_of_subsection(program, builder, params, param_types_):
    arr, sub = params
    arr_type_, sub_type_ = param_types_
    assert arr_type_ == sub_type_
    generic_type_ = arr_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, generic_type_))
    return program.call_extern(
        builder, "indexOfSubsection", [arr, sub, elem_size],
        [ArrayW8, ArrayW8, W64], Z64
    ), Z64


def parse_int(program, builder, params, param_types_):
    string, = params
    return program.call_extern(
        builder, "parseInt", [string], [String], Z32
    ), Z32


def invalid_parsed_int(program, builder, params, param_types_):
    return program.call_extern(
        builder, "getMagicInvalidParsedInt", [], [], Z32
    ), Z32


def parse_float(program, builder, params, param_types_):
    string, = params
    return program.call_extern(
        builder, "parseFloat", [string], [String], Q64
    ), Q64


def is_nan(program, builder, params, param_types_):
    value, = params
    value_type_, = param_types_
    if value_type_["name"] != "Q":
        return i1_of(0), Bool
    return program.call_extern(
        builder, "isNaN64" if value_type_["bits"] >= 64 else "isNaN32",
        [value], [value_type_], Bool
    ), Bool


def read_input_line(program, builder, params, param_types_):
    return program.call_extern(
        builder, "readInputLine", [], [], String
    ), String


def open_file(program, builder, params, param_types_):
    file, mode = params
    return program.call_extern(
        builder, "openFile", [file, mode], [String, Z32], OptionalFile
    ), OptionalFile


def paramless_func(name, type_):
    def inner(program, builder, params, param_types_):
        return program.call_extern(
            builder, name, [], [], type_
        ), type_
    return {"func": inner, "params": []}


def is_file_open(program, builder, params, param_types_):
    file, = params
    return program.call_extern(
        builder, "fileOpen", [file], [File], Bool
    ), Bool


def file_mode(program, builder, params, param_types_):
    file, = params
    return program.call_extern(
        builder, "fileMode", [file], [File], Z32
    ), Z32


def close_file(program, builder, params, param_types_):
    file, = params
    program.call_extern(builder, "closeFile", [file], [File], None)
    return None, None


def file_length(program, builder, params, param_types_):
    file, = params
    return program.call_extern(
        builder, "fileLength", [file], [File], Z64
    ), Z64


def file_pos(program, builder, params, param_types_):
    file, = params
    return program.call_extern(builder, "filePos", [file], [File], Z64), Z64


def file_read_all(program, builder, params, param_types_):
    file, = params
    return program.call_extern(
        builder, "readAllFile", [file], [File], OptionalString
    ), OptionalString


def file_read_some(program, builder, params, param_types_):
    file, amount = params
    return program.call_extern(
        builder, "readSomeFile", [file, amount], [File, W64], OptionalString
    ), OptionalString


def set_file_pos(program, builder, params, param_types_):
    file, pos = params
    return program.call_extern(
        builder, "setFilePos", [file, pos], [File, W64], Bool
    ), Bool


def file_jump_pos(program, builder, params, param_types_):
    file, amount = params
    return program.call_extern(
        builder, "jumpFilePos", [file, amount], [File, W64], Bool
    ), Bool


def read_file_line(program, builder, params, param_types_):
    file, = params
    return program.call_extern(
        builder, "readFileLine", [file], [File], OptionalString
    ), OptionalString


def read_file_lines(program, builder, params, param_types_):
    file, = params
    return program.call_extern(
        builder, "readFileLines", [file], [File], OptionalArrayString
    ), OptionalArrayString


def write_to_file(program, builder, params, param_types_):
    file, text = params
    return program.call_extern(
        builder, "writeToFile", [file, text], [File, String], Bool
    ), Bool


def is_null(program, builder, params, param_types_):
    value, = params
    value_type_, = param_types_
    assert not is_value_type_(value_type_)
    null_ptr = program.nullptr(builder, make_type_(program, value_type_))
    return builder.icmp_unsigned("==", value, null_ptr), Bool


def unwrap(program, builder, params, param_types_):
    value, = params
    value_type_, = param_types_
    generic_type_ = value_type_["generics"][0]
    program.verify_not_null(builder, value)
    return value, generic_type_


def array_unique(program, builder, params, param_types_):
    arr, = params
    arr_type_, = param_types_
    generic_type_ = arr_type_["generics"][0]
    program.sort_array(builder, arr, arr_type_)
    program.dedup(builder, arr, generic_type_)
    return None, None


def sort_array(program, builder, params, param_types_):
    arr, = params
    arr_type_, = param_types_
    program.sort_array(builder, arr, arr_type_)
    return None, None


def sort_array_inverted(program, builder, params, param_types_):
    arr, = params
    arr_type_, = param_types_
    program.sort_array(builder, arr, arr_type_, invert=True)
    return None, None


def dedup(program, builder, params, param_types_):
    arr, = params
    arr_type_, = param_types_
    generic_type_ = arr_type_["generics"][0]
    program.dedup(builder, arr, generic_type_)
    return None, None


def repeat_array(program, builder, params, param_types_):
    arr, times = params
    arr_type_, _ = param_types_
    generic_type_ = arr_type_["generics"][0]
    elem = program.make_elem(builder, generic_type_)
    return program.call_extern(
        builder, "repeatArray", [arr, times, elem],
        [ArrayW8, W64, W64], ArrayW8
    ), ArrayW8


def truthy(program, builder, params, param_types_):
    value, = params
    type_, = param_types_
    return truth_value(program, builder, value, type_), Bool


def floor(program, builder, params, param_types_):
    value, = params
    return program.call_extern(
        builder, "ffloor", [value], [Q64], Z32
    ), Z32


def ceil(program, builder, params, param_types_):
    value, = params
    return program.call_extern(
        builder, "fceil", [value], [Q64], Z32
    ), Z32


def round_(program, builder, params, param_types_):
    value, = params
    return program.call_extern(
        builder, "round", [value], [Q64], Z32
    ), Z32


def inner(program, builder, params, param_types_):
    value, = params
    value_type_, = param_types_
    generic_type_ = value_type_["generics"][0]
    null = ir.Constant(make_type_(program, value_type_), None)
    builder.assume(builder.icmp_unsigned("!=", value, null))
    return value, generic_type_


def nullable_or(program, builder, params, param_types_):
    a, b = params
    a_type_, b_type_ = param_types_
    null_ptr = program.nullptr(builder, make_type_(program, a_type_))
    a_is_null = builder.icmp_unsigned("==", a, null_ptr)
    if b_type_["name"] == "Optional":
        result_type_ = a_type_
    else:
        result_type_ = a_type_["generics"][0]
    b = convert_type_(program, builder, b, b_type_, result_type_)
    return builder.select(a_is_null, b, a), result_type_


def nullable_and(program, builder, params, param_types_):
    a, b = params
    a_type_, b_type_ = param_types_
    a_null_ptr = program.nullptr(builder, make_type_(program, a_type_))
    a_is_null = builder.icmp_unsigned("==", a, a_null_ptr)
    if b_type_["name"] == "Optional":
        result_type_ = b_type_
    else:
        result_type_ = {"name": "Optional", "bits": None, "generics": [b_type_]}
    result_null = program.nullptr(builder, make_type_(program, result_type_))
    return builder.select(a_is_null, result_null, b), result_type_


def nullable_or_w_nonnull(program, builder, params, param_types_):
    a, b = params
    a_type_, b_type_ = param_types_
    generic_type_ = a_type_["generics"][0]
    b = convert_type_(program, builder, b, b_type_, generic_type_)
    return builder.select(a_is_null, b, a), generic_type_


def nullable_or_w_nullable(program, builder, params, param_types_):
    a, b = params
    a_type_, b_type_ = param_types_
    null_ptr = program.nullptr(builder, make_type_(program, a_type_))
    a_is_null = builder.icmp_unsigned("==", a, null_ptr)
    b = convert_type_(program, builder, b, b_type_, a_type_)
    return builder.select(a_is_null, b, a), a_type_


def nullable_and_w_nonnull(program, builder, params, param_types_):
    a, b = params
    a_type_, b_type_ = param_types_
    result_type_ = {"name": "Optional", "bits": None, "generics": [b_type_]}
    a_null_ptr = program.nullptr(builder, make_type_(program, a_type_))
    a_is_null = builder.icmp_unsigned("==", a, a_null_ptr)
    result_null_ptr = program.nullptr(builder, make_type_(program, result_type_))
    return builder.select(a_is_null, result_null_ptr, b), result_type_


def nullable_and_w_nullable(program, builder, params, param_types_):
    a, b = params
    a_type_, b_type_ = param_types_
    a_null_ptr = program.nullptr(builder, make_type_(program, a_type_))
    a_is_null = builder.icmp_unsigned("==", a, a_null_ptr)
    b_null_ptr = program.nullptr(builder, make_type_(program, b_type_))
    return builder.select(a_is_null, b_null_ptr, b), b_type_


def bitwise_invert(program, builder, params, param_types_):
    a, = params
    a_type_, = param_types_
    return builder.not_(a), a_type_


def bitwise_and(program, builder, params, param_types_):
    a, b = params
    type_, b_type_ = param_types_
    b = convert_type_(program, builder, b, b_type_, type_)
    return builder.and_(a, b), type_


def bitwise_or(program, builder, params, param_types_):
    a, b = params
    type_, b_type_ = param_types_
    b = convert_type_(program, builder, b, b_type_, type_)
    return builder.or_(a, b), type_


def bitwise_xor(program, builder, params, param_types_):
    a, b = params
    type_, b_type_ = param_types_
    b = convert_type_(program, builder, b, b_type_, type_)
    return builder.xor(a, b), type_


def bitshift_left(program, builder, params, param_types_):
    a, b = params
    a_type_, b_type_ = param_types_
    return program.bitshift_left(builder, a, b, a_type_, b_type_), a_type_


def bitshift_right(program, builder, params, param_types_):
    a, b = params
    a_type_, b_type_ = param_types_
    return program.bitshift_right(builder, a, b, a_type_, b_type_), a_type_


def unsafe_bitshift_left(program, builder, params, param_types_):
    a, b = params
    type_, b_type_ = param_types_
    b = convert_type_(program, builder, b, b_type_, type_)
    return builder.shl(a, b), type_


def unsafe_bitshift_right(program, builder, params, param_types_):
    a, b = params
    type_, b_type_ = param_types_
    b = convert_type_(program, builder, b, b_type_, type_)
    if is_signed_integer_type_(type_):
        return builder.ashr(a, b), type_
    else:
        return builder.lshr(a, b), type_


BUILTINS = {
    "builtin1": {"func": length, "params": [ArrayW8]},
    "builtin2": {"func": capacity, "params": [ArrayW8]},
    "builtin3": {"func": append, "params": [ArrayW8, None]},
    "builtin4": {"func": require_capacity, "params": [ArrayW8, W64]},
    "builtin5": {"func": shrink_mem, "params": [ArrayW8]},
    "builtin6": {"func": pop, "params": [ArrayW8, W64]}, # result is not in params when builtin finishes
    "builtin7": {"func": insert, "params": [ArrayW8, W64, None]},
    "builtin8": {"func": clone, "params": [None]},
    "builtin9": {"func": extend, "params": [ArrayW8, ArrayW8]},
    "builtin10": {"func": concat, "params": [ArrayW8, ArrayW8]},
    "builtin11": {"func": unsafe_idx, "params": [ArrayW8, W64], "result_in_params": True},
    "builtin12": {"func": unsafe_int_division, "params": [None, None]},
    "builtin13": {"func": unsafe_idx_assign, "params": [ArrayW8, W64, None]},
    "builtin14": {"func": abs_, "params": [Z32]},
    "builtin15": {"func": fabs, "params": [Q64]},
    "builtin16": {"func": concat, "params": [ArrayW8, ArrayW8]},
    "builtin17": {"func": stringify, "params": [None]},
    "builtin18": {"func": print_, "params": [None]},
    "builtin19": {"func": println, "params": [None]},
    "builtin20": {"func": left_pad, "params": [String, W64, Byte]},
    "builtin21": {"func": right_pad, "params": [String, W64, Byte]},
    "builtin22": {"func": slice_, "params": [ArrayW8, W64, W64]},
    "builtin23": {"func": countChr, "params": [String, Byte]},
    "builtin24": {"func": count, "params": [ArrayW8, ArrayW8]},
    "builtin25": {"func": overlapCount, "params": [ArrayW8, ArrayW8]},
    "builtin26": {"func": nest, "params": [ArrayW8]},
    "builtin27": {"func": split, "params": [ArrayW8, ArrayW8]},
    "builtin28": {"func": starts_with, "params": [ArrayW8, ArrayW8]},
    "builtin29": {"func": ends_with, "params": [ArrayW8, ArrayW8]},
    "builtin30": {"func": equals, "params": [None, None]},
    "builtin31": {"func": not_equals, "params": [None, None]},
    "builtin32": {"func": equals_depth, "params": [None, None, None]},
    "builtin33": {"func": not_equals_depth, "params": [None, None, None]},
    "builtin34": {"func": join, "params": [None, None]},
    "builtin35": {"func": index_of, "params": [None, None]},
    "builtin36": {"func": index_of_subsection, "params": [ArrayW8, ArrayW8]},
    "builtin37": {"func": parse_int, "params": [String]},
    "builtin38": {"func": invalid_parsed_int, "params": []},
    "builtin39": {"func": parse_float, "params": [String]},
    "builtin40": {"func": is_nan, "params": [None]},
    "builtin41": {"func": read_input_line, "params": []},
    "builtin42": {"func": open_file, "params": [String, Z32]},
    "builtin43": paramless_func("FILE_READ_MODE", Z32),
    "builtin44": paramless_func("FILE_WRITE_MODE", Z32),
    "builtin45": paramless_func("FILE_APPEND_MODE", Z32),
    "builtin46": paramless_func("FILE_BINARY_MODE", Z32),
    "builtin47": {"func": is_file_open, "params": [File]},
    "builtin48": {"func": file_mode, "params": [File]},
    "builtin49": {"func": close_file, "params": [File]},
    "builtin50": {"func": file_length, "params": [File]},
    "builtin51": {"func": file_pos, "params": [File]},
    "builtin52": {"func": file_read_all, "params": [File]},
    "builtin53": {"func": file_read_some, "params": [File, W64]},
    "builtin54": {"func": set_file_pos, "params": [File, W64]},
    "builtin55": {"func": file_jump_pos, "params": [File, W64]},
    "builtin56": {"func": read_file_line, "params": [File]},
    "builtin57": paramless_func("readLineReachedEOF", Bool),
    "builtin58": {"func": read_file_lines, "params": [File]},
    "builtin59": {"func": write_to_file, "params": [File, String]},
    "builtin60": {"func": is_null, "params": [None]},
    "builtin61": {"func": unwrap, "params": [None], "result_in_params": True, "result_is_param": True},
    "builtin63": {"func": array_unique, "params": [None]},
    "builtin64": {"func": sort_array, "params": [None]},
    "builtin65": {"func": sort_array_inverted, "params": [None]},
    "builtin66": {"func": dedup, "params": [None]},
    "builtin67": {"func": repeat_array, "params": [ArrayW8, W64]},
    "builtin68": {"func": truthy, "params": [None]},
    "builtin69": {"func": floor, "params": [Q64]},
    "builtin70": {"func": ceil, "params": [Q64]},
    "builtin71": {"func": round_, "params": [Q64]},
    "builtin72": {"func": inner, "params": [None], "result_in_params": True, "result_is_param": True},
    "builtin73": {"func": nullable_or, "params": [None, None], "result_in_params": True},
    "builtin74": {"func": nullable_and, "params": [None, None], "result_in_params": True},
    "builtin75": {"func": bitwise_invert, "params": [None]},
    "builtin76": {"func": bitwise_and, "params": [None, None]},
    "builtin77": {"func": bitwise_or, "params": [None, None]},
    "builtin78": {"func": bitwise_xor, "params": [None, None]},
    "builtin79": {"func": bitshift_left, "params": [None, None]},
    "builtin80": {"func": bitshift_right, "params": [None, None]},
    "builtin81": {"func": unsafe_bitshift_left, "params": [None, None]},
    "builtin82": {"func": unsafe_bitshift_right, "params": [None, None]},
}
