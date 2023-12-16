from llvmlite import ir
from common import *


def refrence_equals(program, i, type_, invert=False):
    ir_type = make_type_(program, type_)
    bool_ir_type = make_type_(program, Bool)
    func = ir.Function(
        program.module, ir.FunctionType(
            bool_ir_type, [ir_type, ir_type]
        ), name=f"ref_eq{i}"
    )
    entry = func.append_basic_block(name="entry")
    v1, v2 = func.args
    builder = ir.IRBuilder(entry)
    
    comparer = "!=" if invert else "=="
    if is_number_type_(type_):
        builder.ret(compare_values(builder, comparer, v1, v2, type_))
    else:
        builder.ret(builder.icmp_unsigned(
            comparer, v1, v2
        ))
    
    return func


def value_equals_depth_1(program, i, type_, invert=False):
    ir_type = make_type_(program, type_)
    bool_ir_type = make_type_(program, Bool)
    func = ir.Function(
        program.module, ir.FunctionType(
            bool_ir_type, [ir_type, ir_type]
        ), name=f"val_eq{i}"
    )
    entry = func.append_basic_block(name="entry")
    v1, v2 = func.args
    builder = ir.IRBuilder(entry)

    if is_number_type_(type_):
        builder.ret(compare_values(
            builder, "!=" if invert else "==", v1, v2, type_
        ))
    if type_["name"] == "Array":
        generic_type_ = type_["generics"][0]
        elem_size = program.sizeof(
            builder, make_type_(program, generic_type_)
        )
        result = builder.trunc(program.call_extern(
            builder, "arrayEqual", [v1, v2, elem_size], 
            [type_, type_, W64], Z32
        ), bool_ir_type)
        if invert:
            result = builder.not_(result)
        builder.ret(result)
    else:
        size = program.sizeof(
            builder, ir_type.pointee
        )
        cmp = program.call_extern(
            builder, "memcmp", [v1, v2, size], 
            [type_, type_, W64], Z32
        )
        builder.ret(builder.icmp_signed(
            "!=" if invert else "==", cmp, i32_of(0)
        ))

    return func
