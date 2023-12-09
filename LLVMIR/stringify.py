from llvmlite import ir
from common import *


# TODO: so much more testing of this function
def make_stringify_func(program, type_, i):
    ir_type = make_type_(program, type_)
    func = ir.Function(
        program.module, ir.FunctionType(make_type_(program, String), [ir_type]),
        name=f"str{i}"
    )
    entry = func.append_basic_block(name="entry")
    val, = func.args
    builder = ir.IRBuilder(entry)

    byte_ir_type = make_type_(program, Byte)

    struct_mem = program.malloc(
        builder, make_type_(program, String).pointee
    )
    init_ref_counter(builder, struct_mem)

    if type_ == Bool:
        capacity_field = builder.gep(struct_mem, [i64_of(0), i32_of(1)])
        length_field = builder.gep(struct_mem, [i64_of(0), i32_of(2)])
        array_field = builder.gep(struct_mem, [i64_of(0), i32_of(3)])
        true_block = func.append_basic_block(name="true")
        false_block = func.append_basic_block(name="false")
        builder.cbranch(val, true_block, false_block)
        true_builder = ir.IRBuilder(true_block)
        false_builder = ir.IRBuilder(false_block)
        for cond in [True, False]:
            text = {
                False: "false",
                True: "true"
            }[cond]
            capacity = len(text)
            abuilder = true_builder if cond else false_builder
            abuilder.store(i64_of(capacity), capacity_field)
            abuilder.store(i64_of(capacity), length_field)
            array_mem = program.malloc(program, byte_ir_type, capacity)
            for i, char in enumerate(text):
                abuilder.store(i8_of(ord(char)), abuilder.gep(
                    array_mem, [i64_of(i)]
                ))
            abuilder.store(array_mem, array_field)
            abuilder.ret(struct_mem)
            
    elif type_ == Byte:
        builder.store(
            i64_of(1),
            builder.gep(struct_mem, [i64_of(0), i32_of(1)])
        )
        builder.store(
            i64_of(1),
            builder.gep(struct_mem, [i64_of(0), i32_of(2)])
        )
        array_mem = program.malloc(
            builder, byte_ir_type
        )
        builder.store(val, array_mem)
        builder.store(
            array_mem,
            builder.gep(struct_mem, [i64_of(0), i32_of(3)]),
        )
        builder.ret(struct_mem)
        
    elif is_number_type_(type_):
        # TODO: test to make sure specifiers are correct
        specifier = ""
        casted_value = val
        bits = type_["bits"]
        
        if is_floating_type_(type_):
            if bits < 32:
                casted_value = convert_type_(program, builder, val, type_, Q32)
                specifier = "%f"
            else:
                casted_value = convert_type_(program, builder, val, type_, Q64)
                specifier = "%lf"
                
        elif is_integer_type_(type_):
            name = type_["name"]
            new_type_ = {"name": name, "bits": 8, "generics": []}
            if bits > 32:
                new_type_["bits"] = 64
                specifier += "%l"
            elif bits > 16:
                new_type_["bits"] = 32
                specifier = "%"
            elif bits > 8:
                new_type_["bits"] = 16
                specifier = "%h"
            specifier += {"Z": "d", "W": "u"}[name] + "\0"
            casted_value = convert_type_(program, builder, val, type_, new_type_)
            
        size = len(specifier)+1
        specifier_mem = program.malloc(
            builder, byte_ir_type, size
        )
        for i, char in enumerate(specifier):
            builder.store(i8_of(ord(char)), builder.gep(specifier_mem, [i64_of(i)]))
        length = builder.add(program.call_extern(
            builder, "snprintf", [
                program.nullptr(builder, byte_ir_type.as_pointer()), i64_of(0), 
                specifier_mem
            ], [PointerW8, W64, PointerW8], W64, [casted_value]
        ), i64_of(1))
        builder.store(length, builder.gep(struct_mem, [i64_of(0), i32_of(1)]))
        builder.store(length, builder.gep(struct_mem, [i64_of(0), i32_of(2)]))
        array_mem = program.mallocv(builder, byte_ir_type, length)
        program.call_extern(
            builder, "snprintf", [
                array_mem, length, specifier_mem
            ], [PointerW8, W64, PointerW8], VOID, [casted_value]
        )
        builder.store(array_mem, builder.gep(struct_mem, [i64_of(0), i32_of(3)]))
        builder.ret(struct_mem)

    # TODO: add array and struct support
    
    return func
