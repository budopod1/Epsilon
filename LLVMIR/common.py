import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path


REF_COUNTER_FIELD = ir.IntType(64)


def traverse(json):
    yield json
    if isinstance(json, list):
        for elem in json:
            yield from traverse(elem)
    elif isinstance(json, dict):
        for elem in json.values():
            yield from traverse(elem)


def freeze_json(json):
    if isinstance(json, list):
        return tuple([
            freeze_json(elem) for elem in json
        ])
    elif isinstance(json, dict):
        return frozenset({
            (key, freeze_json(value))
            for key, value in json.items()
        })
    return json


def make_type_(program, data):
    generics = [
        make_type_(program, generic)
        for generic in data["generics"]
    ]

    bits = data["bits"]
    match data["name"], generics:
        case "Q", []:
            return {
                16: ir.HalfType,
                32: ir.FloatType,
                64: ir.DoubleType
            }[bits]()
        case ("W" | "Z" | "Bool" | "Byte") as name, []:
            if bits is None:
                bits = {
                    "W": 32, "Z": 32, "Bool": 1, "Byte": 8
                }[name]
            return ir.IntType(bits)
        case "Void", []:
            return ir.VoidType()
        case "Pointer", [pointee]:
            return pointee.as_pointer()
        case "Func", [ret, *params]:
            return ir.FunctionType(ret, params)
        case "Array", [sub]:
            id_ = program.array_ids[freeze_json(data)]
            return ir.global_context.get_identified_type(
                "a"+str(id_)
            ).as_pointer()
        case "Optional", [sub]:
            return sub
        case "File", []:
            return ir.global_context.get_identified_type(
                "struct.File"
            ).as_pointer()
        case "Null", []:
            return ir.IntType(8).as_pointer()
        case name, []:
            return ir.global_context.get_identified_type(
                "___"+name
            ).as_pointer()

    assert False, f"Invalid type {data}"


def make_constant(constant, ir_type):
    ir_const = ir.Constant(
        ir_type,
        constant["value"]
    )
    ir_const.json_const = constant
    return ir_const


def is_value_type_(type_):
    return type_["name"] in ["W", "Z", "Bool", "Byte", "Q", "Void", "Null"]


def is_signed_integer_type_(type_):
    return type_["name"] in ["Z"]


def is_unsigned_integer_type_(type_):
    return type_["name"] in ["W", "Bool", "Byte"]


def is_integer_type_(type_):
    return is_signed_integer_type_(type_) or is_unsigned_integer_type_(type_)


def is_floating_type_(type_):
    return type_["name"] in ["Q"]


def is_number_type_(type_):
    return is_integer_type_(type_) or is_floating_type_(type_)


def is_nullable_type_(type_):
    return type_["name"] in ["File", "Optional"]


def convert_floating_type__bits(builder, val, old, new, new_type):
    oldb = old["bits"]
    newb = new["bits"]
    if newb > oldb:
        return builder.fpext(val, new_type)
    elif newb < oldb:
        return builder.fptrunc(val, new_type)
    else:
        return val


def convert_integer_type__bits(builder, val, old, new, new_type):
    oldb = old["bits"]
    newb = new["bits"]
    if newb > oldb:
        if is_signed_integer_type_(old):
            return builder.sext(val, new_type)
        else:
            return builder.zext(val, new_type)
    elif newb < oldb:
        return builder.trunc(val, new_type)
    else:
        return val


def is_void_pointer(type_):
    return type_["name"] == "W" and type_["bits"] == 8


def convert_type_(program, builder, val, old, new):
    if old == new:
        return val
    if old["name"] == "Optional" and old["generics"][0] == new:
        return val
    if new["name"] == "Optional" and new["generics"][0] == old:
        return val
    if new["name"] == "Bool":
        return truth_value(program, builder, val, old)
    new_ir_type = make_type_(program, new)
    if is_integer_type_(old) and is_integer_type_(new):
        return convert_integer_type__bits(builder, val, old, new, new_ir_type)
    elif is_floating_type_(old) and is_floating_type_(new):
        return convert_floating_type__bits(builder, val, old, new, new_ir_type)
    elif is_floating_type_(old) and is_signed_integer_type_(new):
        return builder.fptosi(val, new_ir_type)
    elif is_floating_type_(old) and is_unsigned_integer_type_(new):
        return builder.fptoui(val, new_ir_type)
    elif is_signed_integer_type_(old) and is_floating_type_(new):
        return builder.sitofp(val, new_ir_type)
    elif is_unsigned_integer_type_(old) and is_floating_type_(new):
        return builder.uitofp(val, new_ir_type)
    elif ((not is_value_type_(old) or old == Null) 
          and (not is_value_type_(new))):
        return builder.bitcast(val, new_ir_type)
    raise TypeError(f"Cannot convert type {old} to {new}")


def compare_values(builder, comparison, value1, value2, type_):
    if is_floating_type_(type_):
        return builder.fcmp_unordered(comparison, value1, value2)
    elif is_signed_integer_type_(type_):
        return builder.icmp_signed(comparison, value1, value2)
    else:
        return builder.icmp_unsigned(comparison, value1, value2)


def truth_value(program, builder, value, type_):
    if is_integer_type_(type_) and type_["bits"] == 1:
        return value
    elif is_floating_type_(type_):
        return builder.fcmp_unordered("!=", value, ir.Constant(make_type_(program, type_), 0))
    elif is_signed_integer_type_(type_):
        return builder.icmp_signed("!=", value, ir.Constant(make_type_(program, type_), 0))
    elif is_unsigned_integer_type_(type_):
        return builder.icmp_unsigned("!=", value, ir.Constant(make_type_(program, type_), 0))
    elif type_ == Null:
        return i1_of(0)
    elif not is_value_type_(type_):
        null_ptr = program.nullptr(builder, make_type_(program, type_))
        return builder.icmp_unsigned("!=", value, null_ptr)
    else:
        raise ValueError(f"Cannot get truth value of type_ {type_}")


def untruth_value(program, builder, value, type_):
    if is_integer_type_(type_) and type_["bits"] == 1:
        return builder.not_(value)
    elif is_floating_type_(type_):
        return builder.fcmp_unordered("==", value, ir.Constant(make_type_(program, type_), 0))
    elif is_signed_integer_type_(type_):
        return builder.icmp_signed("==", value, ir.Constant(make_type_(program, type_), 0))
    elif is_unsigned_integer_type_(type_):
        return builder.icmp_unsigned("==", value, ir.Constant(make_type_(program, type_), 0))
    elif type_ == Null:
        return i1_of(1)
    elif not is_value_type_(type_):
        null_ptr = program.nullptr(builder, make_type_(program, type_))
        return builder.icmp_unsigned("==", value, null_ptr)
    else:
        raise ValueError(f"Cannot get untruth value of type_ {type_}")


def iter_block_chain(block_chain):
    block = block_chain
    while block is not None:
        yield block
        block = block.next_block


def last_block_chain_block(block_chain):
    result = None
    for block in iter_block_chain(block_chain):
        result = block
    return result


def set_return_block(block_chain, return_block):
    for block in iter_block_chain(block_chain):
        block.return_block = return_block


def make_function_type_(program, return_type_, arguments, vargs=False):
    return ir.FunctionType(
        make_type_(program, return_type_),
        [
            make_type_(program, argument) 
            for argument in arguments
        ], vargs
    )


def i64_of(val):
    return ir.IntType(64)(val)


def i32_of(val):
    return ir.IntType(32)(val)


def i8_of(val):
    return ir.IntType(8)(val)


def i1_of(val):
    return ir.IntType(1)(val)


def init_ref_counter(builder, val):
    builder.store(
        REF_COUNTER_FIELD(0),
        builder.bitcast(val, REF_COUNTER_FIELD.as_pointer())
    )


def incr_ref_counter(program, builder, val, type_, no_nulls=False):
    if is_nullable_type_(type_) and not no_nulls:
        null_ptr = program.nullptr(builder, make_type_(program, type_))
        is_null = builder.icmp_unsigned("!=", val, null_ptr)
        with builder.if_then(is_null):
            incr_ref_counter(program, builder, val, type_, no_nulls=True)
        return None
    else:
        ref_counter = builder.bitcast(val, REF_COUNTER_FIELD.as_pointer())
        incred = builder.add(builder.load(ref_counter), REF_COUNTER_FIELD(1))
        builder.store(incred, ref_counter)
        return incred


def dumb_decr_ref_counter(program, builder, val, type_, no_nulls=False):
    if is_nullable_type_(type_) and not no_nulls:
        null_ptr = program.nullptr(builder, make_type_(program, type_))
        is_null = builder.icmp_unsigned("!=", val, null_ptr)
        with builder.if_then(is_null):
            dumb_decr_ref_counter(program, builder, val, type_, no_nulls=True)
        return None
    else:
        ref_counter = builder.bitcast(val, REF_COUNTER_FIELD.as_pointer())
        decred = builder.sub(
            builder.load(ref_counter), REF_COUNTER_FIELD(1)
        )
        builder.store(decred, ref_counter)
        return decred


def do_chain_power(program, builder, type_, value, pow):
    mul = builder.fmul if is_floating_type_(type_) else builder.mul
    if pow == 0:
        return ir.Constant(make_type_(program, type_), 1)
    elif pow % 2 == 0:
        half = do_chain_power(
            program, builder, type_, value, pow/2
        )
        return mul(half, half)
    else:
        return mul(value, do_chain_power(
            program, builder, type_, value, pow - 1
        ))


def Pointer(type_):
    return {"name": "Pointer", "bits": None, "generics": [type_]}


def Array(type_):
    return {"name": "Array", "bits": None, "generics": [type_]}


def Optional(type_):
    return {"name": "Optional", "bits": None, "generics": [type_]}


def FuncType_(ret_type_, params):
    return {"name": "Func", "bits": None, "generics": [ret_type_, *params]}


bool_true = ir.IntType(1)(1)
bool_false = ir.IntType(1)(0)


VOID = {"name": "Void", "bits": None, "generics": []}
Z32 = {"name": "Z", "bits": 32, "generics": []}
Z64 = {"name": "Z", "bits": 64, "generics": []}
W64 = {"name": "W", "bits": 64, "generics": []}
W32 = {"name": "W", "bits": 32, "generics": []}
W8 = {"name": "W", "bits": 8, "generics": []}
ArrayW8 = Array(W8)
ArrayZ32 = Array(Z32)
PointerW8 = Pointer(W8)
Q64 = {"name": "Q", "bits": 64, "generics": []}
Q32 = {"name": "Q", "bits": 32, "generics": []}
Byte = {"name": "Byte", "bits": 8, "generics": []}
String = Array(Byte)
Bool = {"name": "Bool", "bits": 1, "generics": []}
File = {"name": "File", "bits": None, "generics": []}
OptionalString = Optional(String)
ArrayString = Array(String)
OptionalArrayString = Optional(ArrayString)
ComparerType_ = Pointer(FuncType_(Z32, [PointerW8, PointerW8]))
Null = {"name": "Null", "bits": None, "generics": []}
