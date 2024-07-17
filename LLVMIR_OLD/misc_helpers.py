from llvmlite import ir
from common import *


def index_of(program, i, type_):
    array_ir_type = make_type_(program, Array(type_))
    ir_type = make_type_(program, type_)
    func = ir.Function(
        program.module, ir.FunctionType(
            make_type_(program, Z64), [array_ir_type, ir_type]
        ), name=f"{program.path} index_of{i}"
    )
    arr, target = func.args

    entry = func.append_basic_block(name="entry")
    builder = ir.IRBuilder(entry)

    check_block = func.append_basic_block(name="check")
    cbuilder = ir.IRBuilder(check_block)

    loop_block = func.append_basic_block(name="loop")
    lbuilder = ir.IRBuilder(loop_block)

    match_block = func.append_basic_block(name="match")
    mbuilder = ir.IRBuilder(match_block)

    fail_block = func.append_basic_block(name="fail")
    fbuilder = ir.IRBuilder(fail_block)

    length = builder.load(builder.gep(arr, [i64_of(0), i32_of(2)]))
    content = builder.load(builder.gep(arr, [i64_of(0), i32_of(3)]))

    builder.branch(check_block)

    index = cbuilder.phi(make_type_(program, W64))
    finished = cbuilder.icmp_unsigned("==", index, length)
    cbuilder.cbranch(finished, fail_block, loop_block)

    fbuilder.ret(i64_of(-1))

    elem = lbuilder.load(lbuilder.gep(content, [index]))
    equal = program.refrence_equals(
        lbuilder, type_, elem, target
    )
    next_index = lbuilder.add(index, i64_of(1))
    lbuilder.cbranch(equal, match_block, check_block)

    index.add_incoming(i64_of(0), entry)
    index.add_incoming(next_index, loop_block)

    mbuilder.ret(index)

    return func


def dedup(program, i, type_):
    ir_type = make_type_(program, type_)
    array_ir_type = make_type_(program, Array(type_))
    func = ir.Function(
        program.module, ir.FunctionType(
            ir.VoidType(), [array_ir_type]
        ), name=f"{program.path} dedup{i}"
    )

    entry = func.append_basic_block(name="entry")
    builder = ir.IRBuilder(entry)

    vacant_block = func.append_basic_block(name="vacant")
    vbuilder = ir.IRBuilder(vacant_block)

    begin_block = func.append_basic_block(name="begin")
    bbuilder = ir.IRBuilder(begin_block)

    check_block = func.append_basic_block(name="check")
    cbuilder = ir.IRBuilder(check_block)

    loop_block = func.append_basic_block(name="loop")
    lbuilder = ir.IRBuilder(loop_block)

    unique_block = func.append_basic_block(name="unique")
    ubuilder = ir.IRBuilder(unique_block)

    same_block = func.append_basic_block(name="same")
    sbuilder = ir.IRBuilder(same_block)

    finished_block = func.append_basic_block(name="finished")
    fbuilder = ir.IRBuilder(finished_block)

    arr, = func.args

    length_ptr = builder.gep(arr, [i64_of(0), i32_of(2)])
    length = builder.load(length_ptr)

    vacant = builder.icmp_unsigned("==", length, i64_of(0))
    builder.cbranch(vacant, vacant_block, begin_block)

    vbuilder.ret_void()

    content = bbuilder.load(bbuilder.gep(arr, [i64_of(0), i32_of(3)]))

    first_val = bbuilder.load(content)

    bbuilder.branch(check_block)

    i = cbuilder.phi(make_type_(program, W64), name="i")
    j = cbuilder.phi(make_type_(program, W64), name="j")
    last_val = cbuilder.phi(ir_type)
    finished = cbuilder.icmp_unsigned("==", i, length)
    cbuilder.cbranch(finished, finished_block, loop_block)

    val = lbuilder.load(lbuilder.gep(content, [i]), name="val")
    is_unique = compare_values(lbuilder, "!=", val, last_val, type_)
    next_i = lbuilder.add(i, i64_of(1), name="next_i")
    lbuilder.cbranch(is_unique, unique_block, same_block)

    ubuilder.store(val, ubuilder.gep(content, [j]))
    next_j = ubuilder.add(j, i64_of(1), name="next_j")
    ubuilder.branch(check_block)

    program.decr_ref(sbuilder, val, type_)
    sbuilder.branch(check_block)

    fbuilder.store(j, length_ptr)
    fbuilder.ret_void()

    i.add_incoming(i64_of(1), begin_block)
    i.add_incoming(next_i, same_block)
    i.add_incoming(next_i, unique_block)

    j.add_incoming(i64_of(1), begin_block)
    j.add_incoming(j, same_block)
    j.add_incoming(next_j, unique_block)

    last_val.add_incoming(first_val, begin_block)
    last_val.add_incoming(val, same_block)
    last_val.add_incoming(val, unique_block)

    return func


def compare(program, i, type_, invert):
    PointerW8_ir_type = make_type_(program, PointerW8)
    Z32_ir_type = make_type_(program, Z32)
    ir_type = make_type_(program, type_)
    func = ir.Function(
        program.module, ir.FunctionType(
            Z32_ir_type, [PointerW8_ir_type, PointerW8_ir_type]
        ), name=f"{program.path} comparer{i}"
    )

    entry = func.append_basic_block(name="entry")
    builder = ir.IRBuilder(entry)

    val1, val2 = [
        builder.load(builder.bitcast(pointer, ir_type.as_pointer()))
        for pointer in func.args
    ]

    invert_int = -1 if invert else 1
    with builder.if_then(compare_values(builder, "<", val1, val2, type_)):
        builder.ret(i32_of(-invert_int))
    with builder.if_then(compare_values(builder, ">", val1, val2, type_)):
        builder.ret(i32_of(invert_int))
    builder.ret(i32_of(0))

    return func


def optional_array_access(program, i, arr_type_, elem_type_):
    arr_ir_type_ = make_type_(program, arr_type_)
    elem_ir_type_ = make_type_(program, elem_type_)
    W64_ir_type_ = make_type_(program, W64)
    func = ir.Function(
        program.module, ir.FunctionType(
            elem_ir_type_, [arr_ir_type_, W64_ir_type_]
        ), name=f"{program.path} optional_array_access{i}"
    )

    entry = func.append_basic_block(name="entry")
    builder = ir.IRBuilder(entry)

    arr, idx = func.args

    length_ptr = builder.gep(arr, [i64_of(0), i32_of(2)])
    length = builder.load(length_ptr)

    with builder.if_then(compare_values(builder, ">=", idx, length, W64)):
        builder.ret(ir.Constant(elem_ir_type_, None))

    content_ptr = builder.gep(arr, [i64_of(0), i32_of(3)])
    content = builder.load(content_ptr)

    elem_ptr = builder.gep(content, [idx])
    builder.ret(builder.load(elem_ptr))

    return func


def make_unsigned_bitshift_func_body(program, builder, a, b, type_, b_type_, ir_type_, b_ir_type_, is_left):
    shift_too_large = builder.icmp_unsigned(">=", b, ir.Constant(b_ir_type_, ir_type_.width))
    with builder.if_then(shift_too_large):
        builder.ret(ir.Constant(ir_type_, 0))

    b = convert_type_(program, builder, b, b_type_, type_)
    if is_left:
        builder.ret(builder.shl(a, b))
    else:
        if is_signed_integer_type_(type_):
            builder.ret(builder.ashr(a, b))
        else:
            builder.ret(builder.lshr(a, b))


def bitshift(program, i, type_, b_type_, is_left):
    ir_type_ = make_type_(program, type_)
    b_ir_type_ = make_type_(program, b_type_)
    func = ir.Function(
        program.module, ir.FunctionType(
            ir_type_, [ir_type_, b_ir_type_]
        ), name=f"{program.path} bitshift_left{i}"
    )

    entry = func.append_basic_block(name="entry")
    builder = ir.IRBuilder(entry)

    a, b = func.args

    if is_signed_integer_type_(b_type_):
        reverse_op = builder.icmp_signed("<", b, ir.Constant(b_ir_type_, 0))
        with builder.if_then(reverse_op):
            make_unsigned_bitshift_func_body(
                program, builder, a, builder.neg(b), type_, b_type_,
                ir_type_, b_ir_type_, not is_left
            )
        make_unsigned_bitshift_func_body(
            program, builder, a, b, type_, b_type_,
            ir_type_, b_ir_type_, is_left
        )
    else:
        make_unsigned_bitshift_func_body(
            program, builder, a, b, type_, b_type_, ir_type_, b_ir_type_, is_left
        )

    return func
