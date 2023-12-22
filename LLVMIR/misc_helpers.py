from llvmlite import ir
from common import *


def index_of(program, i, type_):
    array_ir_type = make_type_(program, Array(type_))
    ir_type = make_type_(program, type_)
    func = ir.Function(
        program.module, ir.FunctionType(
            make_type_(program, Z64), [array_ir_type, ir_type]
        ), name=f"index_of{i}"
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
