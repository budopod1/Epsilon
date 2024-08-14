from llvmlite import ir
from common import *


def refrence_equals(program, i, type_, invert=False):
    ir_type = make_type_(program, type_)
    bool_ir_type = make_type_(program, Bool)
    func = ir.Function(
        program.module, ir.FunctionType(
            bool_ir_type, [ir_type, ir_type]
        ), name=f"{program.path} ref_eq{i}"
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
        ), name=f"{program.path} val_eq_d1{i}"
    )
    entry = func.append_basic_block(name="entry")
    v1, v2 = func.args
    builder = ir.IRBuilder(entry)

    if is_number_type_(type_):
        builder.ret(compare_values(
            builder, "!=" if invert else "==", v1, v2, type_
        ))

    elif type_["name"] == "Array":
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

    elif type_ == Null:
        builder.ret(i1_of(1))

    elif type_["name"] == "Optional":
        generic_type_ = type_["generics"][0]
        null_ptr = program.nullptr(builder, make_type_(program, type_))
        v1_null = builder.icmp_unsigned("==", v1, null_ptr)
        v2_null = builder.icmp_unsigned("==", v2, null_ptr)
        with builder.if_else(v1_null) as (then, otherwise):
            with then:
                if invert:
                    builder.ret(builder.not_(v2_null))
                else:
                    builder.ret(v2_null)
            with otherwise:
                with builder.if_else(v2_null) as (then, otherwise):
                    with then:
                        builder.ret(i1_of(int(invert)))
                    with otherwise:
                        builder.ret(program.value_equals_depth_1(
                            builder, generic_type_, v1, v2, invert
                        ))
        builder.unreachable()

    elif type_ == Internal:
        builder.ret(i1_of(0))

    else:
        if is_nullable_type_(type_):
            null_ptr = program.nullptr(builder, make_type_(program, type_))
            v1_null = builder.icmp_unsigned("==", v1, null_ptr)
            v2_null = builder.icmp_unsigned("==", v2, null_ptr)
            with builder.if_then(builder.or_(v1_null, v2_null)):
                builder.ret(builder.and_(v1_null, v2_null))

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


def value_equals(program, i, type_, depth, invert=False):
    assert type_["name"] == "Array"
    generic_type_ = type_["generics"][0]

    ir_type = make_type_(program, type_)
    bool_ir_type = make_type_(program, Bool)

    func = ir.Function(
        program.module, ir.FunctionType(
            bool_ir_type, [ir_type, ir_type]
        ), name=f"{program.path} val_eq{i}"
    )
    v1, v2 = func.args

    entry = func.append_basic_block(name="entry")
    builder = ir.IRBuilder(entry)

    check_block = func.append_basic_block(name="check")
    cbuilder = ir.IRBuilder(check_block)

    loop_block = func.append_basic_block(name="loop")
    lbuilder = ir.IRBuilder(loop_block)

    equal_block = func.append_basic_block(name="equal")
    ebuilder = ir.IRBuilder(equal_block)

    unequal_block = func.append_basic_block(name="unequal")
    ubuilder = ir.IRBuilder(unequal_block)

    length1 = builder.load(builder.gep(
        v1, [i64_of(0), i32_of(2)]
    ))
    length2 = builder.load(builder.gep(
        v2, [i64_of(0), i32_of(2)]
    ))

    content1 = builder.load(builder.gep(
        v1, [i64_of(0), i32_of(3)]
    ))
    content2 = builder.load(builder.gep(
        v2, [i64_of(0), i32_of(3)]
    ))

    equal_lens = builder.icmp_unsigned("==", length1, length2)
    builder.cbranch(equal_lens, check_block, unequal_block)

    index = cbuilder.phi(make_type_(program, W64))
    finished = cbuilder.icmp_unsigned("==", index, length1)
    cbuilder.cbranch(finished, equal_block, loop_block)

    item1 = lbuilder.load(lbuilder.gep(content1, [index]))
    item2 = lbuilder.load(lbuilder.gep(content2, [index]))
    item_equality = program.value_equals(
        lbuilder, generic_type_, item1, item2, depth-1
    )
    next_index = lbuilder.add(index, i64_of(1))
    lbuilder.cbranch(item_equality, check_block, unequal_block)

    ebuilder.ret(i1_of(int(not invert)))

    ubuilder.ret(i1_of(int(invert)))

    index.add_incoming(i64_of(0), entry)
    index.add_incoming(next_index, loop_block)

    return func
