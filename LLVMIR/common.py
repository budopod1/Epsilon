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
        case "Array", [sub]:
            id_ = program.array_ids[freeze_json(data)]
            return ir.global_context.get_identified_type(
                "a"+str(id_)
            ).as_pointer()
        case name, []:
            return ir.global_context.get_identified_type(
                "___"+name
            ).as_pointer()

    assert False, f"Invalid type {data}"


def make_constant(constant, ir_type):
    return ir.Constant(
        ir_type,
        constant["value"]
    )


def is_value_type_(type_):
    return type_["name"] in ["W", "Z", "Bool", "Byte", "Q"]


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


def types__equal(type_1, type_2):
    if type_1["name"] != type_2["name"]:
        return False
    if type_1["bits"] != type_2["bits"]:
        return False
    t1_generics = type_1["generics"]
    t2_generics = type_2["generics"]
    if len(t1_generics) != len(t2_generics):
        return False
    generic_equality = [
        types__equal(t1, t2)
        for t1, t2 in zip(t1_generics, t2_generics)
    ]
    return all(generic_equality)


def convert_type_(program, builder, val, old, new):
    if types__equal(old, new):
        return val
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
    elif (not is_value_type_(old)) and (not is_value_type_(new)):
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
    if is_floating_type_(type_):
        return builder.fcmp_unordered("!=", value, ir.Constant(make_type_(program, type_), 0))
    elif is_signed_integer_type_(type_):
        return builder.icmp_signed("!=", value, ir.Constant(make_type_(program, type_), 0))
    else:
        return builder.icmp_unsigned("!=", value, ir.Constant(make_type_(program, type_), 0))


def untruth_value(program, builder, value, type_):
    if is_integer_type_(type_) and type_["bits"] == 1:
        return builder.not_(value)
    if is_floating_type_(type_):
        return builder.fcmp_unordered("==", value, ir.Constant(make_type_(program, type_), 0))
    elif is_signed_integer_type_(type_):
        return builder.icmp_signed("==", value, ir.Constant(make_type_(program, type_), 0))
    else:
        return builder.icmp_unsigned("==", value, ir.Constant(make_type_(program, type_), 0))


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


def init_ref_counter(builder, val):
    builder.store(
        REF_COUNTER_FIELD(0),
        builder.bitcast(val, REF_COUNTER_FIELD.as_pointer())
    )


def incr_ref_counter(builder, val):
    ref_counter = builder.bitcast(val, REF_COUNTER_FIELD.as_pointer())
    incred = builder.add(builder.load(ref_counter), REF_COUNTER_FIELD(1))
    builder.store(incred, ref_counter)


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


def fill_type_(type_):
    return {
        "name": type_["name"], 
        "bits": type_.get("bits", None), 
        "generics": [
            fill_type_(generic) 
            for generic in type_.get("generics", [])
        ]
    }


bool_true = ir.IntType(1)(1)
bool_false = ir.IntType(1)(0)


VOID = {"name": "Void", "bits": None, "generics": []}
Z32 = {"name": "Z", "bits": 32, "generics": []}
W64 = {"name": "W", "bits": 64, "generics": []}
W32 = {"name": "W", "bits": 32, "generics": []}
W8 = {"name": "W", "bits": 8, "generics": []}
ArrayW8 = {"name": "Array", "bits": None, "generics": [W8]}
ArrayZ32 = {"name": "Array", "bits": None, "generics": [Z32]}
PointerW8 = {"name": "Pointer", "bits": None, "generics": [W8]}
Q64 = {"name": "Q", "bits": 64, "generics": []}
Q32 = {"name": "Q", "bits": 32, "generics": []}
Byte = {"name": "Byte", "bits": 8, "generics": []}
String = {"name": "Array", "bits": None, "generics": [Byte]}
Bool = {"name": "Bool", "bits": None, "generics": []}
