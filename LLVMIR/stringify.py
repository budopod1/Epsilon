from llvmlite import ir
from common import *


def stringify_type_(type_):
    generics_inner = ", ".join(map(stringify_type_, type_["generics"]))
    generics = f"<{generics_inner}>" if type_["generics"] else ""
    return f"{type_['name']}{type_['bits'] or ''}{generics}"


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
        builder, make_type_(program, String).pointee, name="struct_mem"
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
            abuilder = true_builder if cond else false_builder
            abuilder.store(i64_of(capacity), capacity_field)
            abuilder.store(i64_of(capacity), length_field)
            abuilder.store(program.string_literal_array(
                abuilder, text, unique=True
            ), array_field)
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
        specifier = None
        casted_value = val
        bits = type_["bits"]
        
        if is_floating_type_(type_):
            if bits < 32:
                casted_value = convert_type_(program, builder, val, type_, Q32)
                specifier_txt = "%f\0"
            else:
                casted_value = convert_type_(program, builder, val, type_, Q64)
                specifier_txt = "%lf\0"
            specifier = program.string_literal_array(
                builder, specifier_txt, name="specifier"
            )
                
        elif is_integer_type_(type_):
            name = type_["name"]
            new_type_ = {"name": name, "bits": 8, "generics": []}
            if bits > 32:
                new_type_["bits"] = 64
            elif bits > 16:
                new_type_["bits"] = 32
            elif bits > 8:
                new_type_["bits"] = 16
            else:
                new_type_["bits"] = 8
            func_name = f"format{name}{new_type_['bits']}"
            specifier = program.call_extern(
                builder, func_name, [], [], PointerW8
            )
            casted_value = convert_type_(program, builder, val, type_, new_type_)
        
        length = program.call_extern(
            builder, "snprintf", [
                program.nullptr(builder, byte_ir_type.as_pointer()), i64_of(0), 
                specifier
            ], [PointerW8, W64, PointerW8], W64, [casted_value]
        )
        null_len = builder.add(length, i64_of(1))
        builder.store(null_len, builder.gep(struct_mem, [i64_of(0), i32_of(1)]))
        builder.store(length, builder.gep(struct_mem, [i64_of(0), i32_of(2)]))
        array_mem = program.mallocv(builder, byte_ir_type, null_len)
        program.call_extern(
            builder, "sprintf", [
                array_mem, specifier
            ], [PointerW8, PointerW8], VOID, [casted_value]
        )
        builder.store(array_mem, builder.gep(struct_mem, [i64_of(0), i32_of(3)]))
        builder.ret(struct_mem)

    elif type_["name"] == "Array":
        start_str = f"{stringify_type_(type_)} ["
        start_capacity = len(start_str)+1
        start_array_mem = program.string_literal_array(
            builder, start_str, start_capacity, unique=True, name="type_name"
        )
        length = builder.load(builder.gep(val, [i64_of(0), i32_of(2)]), name="length")
        content = builder.load(builder.gep(val, [i64_of(0), i32_of(3)]), name="content")
        
        check_block = func.append_basic_block(name="check")
        loop_block = func.append_basic_block(name="loop")
        finish_block = func.append_basic_block(name="finish")
        
        builder.branch(check_block)
        
        cbuilder = ir.IRBuilder(check_block)
        array = cbuilder.phi(byte_ir_type.as_pointer(), name="array")
        array.add_incoming(start_array_mem, entry)
        i = cbuilder.phi(make_type_(program, W64), name="i")
        i.add_incoming(i64_of(0), entry)
        is_first = cbuilder.phi(make_type_(program, Bool), name="is_first")
        is_first.add_incoming(bool_true, entry)
        is_first.add_incoming(bool_false, loop_block)
        capacity = cbuilder.phi(make_type_(program, W64), name="capacity")
        capacity.add_incoming(i64_of(start_capacity), entry)
        cont = cbuilder.icmp_unsigned("==", i, length, name="cont")
        cbuilder.cbranch(cont, finish_block, loop_block)

        lbuilder = ir.IRBuilder(loop_block)
        generic = type_["generics"][0]
        item = lbuilder.load(lbuilder.gep(content, [i], name="item_ptr"), name="item")
        segment = program.stringify(lbuilder, item, generic)
        segment_len = lbuilder.load(
            lbuilder.gep(segment, [i64_of(0), i32_of(2)]), name="segment_len"
        )
        segment_content = lbuilder.load(
            lbuilder.gep(segment, [i64_of(0), i32_of(3)]), name="segment_content"
        )
        combined_cap = lbuilder.add(capacity, segment_len, name="combined_cap")
        shifted_cap = lbuilder.sub(combined_cap, i64_of(1), name="shifted_cap")
        new_cap = lbuilder.add(combined_cap, i64_of(2), name="new_cap")
        new_array = program.call_extern(
            lbuilder, "realloc", [array, new_cap], [PointerW8, W64], PointerW8
        )
        decr_cap = lbuilder.sub(capacity, i64_of(1), name="decr_cap")
        program.call_extern(
            lbuilder, "memcpy", [
                lbuilder.gep(new_array, [decr_cap]), segment_content, segment_len
            ], [
                PointerW8, PointerW8, W64
            ], VOID
        )
        pos1 = shifted_cap
        lbuilder.store(i8_of(ord(",")), lbuilder.gep(new_array, [pos1], name="comma_idx"))
        pos2 = lbuilder.add(shifted_cap, i64_of(1))
        lbuilder.store(i8_of(ord(" ")), lbuilder.gep(new_array, [pos2], name="space_idx"))
        next_i = lbuilder.add(i, i64_of(1), name="next_i")
        program.dumb_free(lbuilder, segment_content)
        program.dumb_free(lbuilder, segment)
        lbuilder.branch(check_block)
        
        array.add_incoming(new_array, loop_block)
        i.add_incoming(next_i, loop_block)
        capacity.add_incoming(new_cap, loop_block)

        fbuilder = ir.IRBuilder(finish_block)
        end_offset = fbuilder.select(is_first, i64_of(0), i64_of(2), name="end_offset")
        str_len = fbuilder.sub(capacity, end_offset, name="str_len")
        last_pos = fbuilder.sub(str_len, i64_of(1), name="last_pos")
        fbuilder.store(i8_of(ord("]")), fbuilder.gep(array, [last_pos]))
        fbuilder.store(capacity, fbuilder.gep(struct_mem, [i64_of(0), i32_of(1)]))
        fbuilder.store(str_len, fbuilder.gep(struct_mem, [i64_of(0), i32_of(2)]))
        fbuilder.store(array, fbuilder.gep(struct_mem, [i64_of(0), i32_of(3)]))
        fbuilder.ret(struct_mem)

    else:
        # It's a struct!
        name = type_["name"]
        struct = program.structs[name]
        if len(struct.fields) == 0:
            result_str = f"{name} []"
            array_mem = program.string_literal_array(
                builder, result_str, unique=True
            )
            builder.store(
                i64_of(len(result_str)), builder.gep(struct_mem, [i64_of(0), i32_of(1)])
            )
            builder.store(
                i64_of(len(result_str)), builder.gep(struct_mem, [i64_of(0), i32_of(2)])
            )
            builder.store(array_mem, builder.gep(struct_mem, [i64_of(0), i32_of(3)]))
            builder.ret(struct_mem)
        else:
            start_str = f"{name} ["
            start_len = len(start_str)
            start_str_mem = program.string_literal_array(
                builder, start_str, name="start_str"
            )
            item_strs = []
            item_lens = []
            item_contents = []
            
            for i, field in enumerate(struct.get_field_types_()):
                item = builder.load(builder.gep(val, [i64_of(0), i32_of(i+1)]), name="item")
                item_str = program.stringify(builder, item, field)
                item_len = builder.load(builder.gep(
                    item_str, [i64_of(0), i32_of(2)]
                ), name="item_len")
                item_content = builder.load(builder.gep(
                    item_str, [i64_of(0), i32_of(3)]
                ), name="item_content")
                item_strs.append(item_str)
                item_lens.append(item_len)
                item_contents.append(item_content)
    
            comma_starts = []
            starts = []
            total = i64_of(start_len)
            for i, value in enumerate(item_lens):
                starts.append(total)
                comma_start = builder.add(total, value, name=f"comma_start{i}")
                comma_starts.append(comma_start)
                total = builder.add(comma_start, i64_of(2), name=f"total{i}")
    
            last_index = comma_starts[-1] # no last ", "
    
            total_length = builder.add(last_index, i64_of(1), name="total_length")
    
            array_mem = program.mallocv(
                builder, byte_ir_type, total_length, name="array_mem"
            )
    
            program.call_extern(
                builder, "memcpy", [array_mem, start_str_mem, i64_of(start_len)],
                [PointerW8, PointerW8, W64], VOID
            )
            builder.store(i8_of(ord("]")), builder.gep(
                array_mem, [last_index], name="closing_square_bracket_ptr"
            ))
    
            for idx, item_len, item_content in zip(starts, item_lens, item_contents):
                program.call_extern(
                    builder, "memcpy", [
                        builder.gep(array_mem, [idx]), item_content, item_len
                    ], [PointerW8, PointerW8, W64], VOID
                )
    
            for i, idx in enumerate(comma_starts[:-1]):
                builder.store(i8_of(ord(",")), builder.gep(
                    array_mem, [idx], name=f"comma_ptr{i}"
                ))
                builder.store(i8_of(ord(" ")), builder.gep(
                    array_mem, [builder.add(idx, i64_of(1))], name=f"space_ptr{i}"
                ))
            
            builder.store(total_length, builder.gep(struct_mem, [i64_of(0), i32_of(1)]))
            builder.store(total_length, builder.gep(struct_mem, [i64_of(0), i32_of(2)]))
            builder.store(array_mem, builder.gep(struct_mem, [i64_of(0), i32_of(3)]))
    
            for item_str in item_strs:
                program.dumb_free(builder, item_str)
            for item_content in item_contents:
                program.dumb_free(builder, item_content)
    
            builder.ret(struct_mem)
    
    return func
