from llvmlite import ir
from common import *


def length(program, builder, params, param_types_):
    param, = params
    result_ptr = builder.gep(param, [i64_of(0), i32_if(2)])
    return builder.load(result_ptr), {"name": "W", "bits": 64}


def capacity(program, builder, params, param_types_):
    param, = params
    result_ptr = builder.gep(param, [i64_of(0), i32_if(1)])
    return builder.load(result_ptr), {"name": "W", "bits": 64}


def append(program, builder, params, param_types_):
    array, value = params
    array_type_, value_type_ = param_types_
    length_ptr = builder.gep(array, [i64_of(0), i32_of(2)])
    length = builder.load(length_ptr)
    elem_type_ = array_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, array_generic))
    program.call_extern(
        builder, "incrementLength", [array, elem_size], [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]},
            {"name": "W", "bits": 8}
        ], {"name": "Void"}
    )
    content_ptr = builder.gep(param, [i64_of(0), i32_of(3)])
    content = builder.bitcast(
        builder.load(content_ptr), 
        make_type_(program, elem_type_).as_pointer()
    )
    end_ptr = builder.gep(content, [length])
    value_casted = convert_type_(program, builder, value, value_type_, elem_type_)
    builder.store(value_casted, end_ptr)
    return None, {"name": "Void"}


def require_capacity(program, builder, params, param_types_):
    array, capacity = params
    array_type_, _ = param_types_
    elem_type_ = array_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, elem_type_))
    program.call_extern(
        builder, "requireCapacity", [], [array, capacity, elem_size], [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]},
            {"name": "W", "bits": 64},
            {"name": "W", "bits": 64}
        ], {"name": "Void"}
    )
    return None, {"name": "Void"}


def shrink_mem(program, builder, params, param_types_):
    array, = params
    array_type_, = param_types_
    elem_type_ = array_type_["generics"][0]
    elem_size = program.sizeof(builder, make_type_(program, elem_type_))
    program.call_extern(
        builder, "shrinkMem", [], [array, elem_size], [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "W", "bits": 64}
        ], {"name": "Void"}
    )
    return None, {"name": "Void"}


def pop(program, builder, params, param_types_):
    array, idx = params
    array_type_, _ = param_types_
    elem_type_ = array_type_["generics"][0]
    elem_ir_type = make_type_(program, elem_type_)
    content_ptr = builder.load(builder.gep(array, [i64_of(0), i32_of(3)]))
    content_ptr_casted = builder.bicast(content_ptr, elem_ir_type.as_pointer())
    elem = builder.load(builder.gep(content_ptr_casted, [idx]))
    elem_size = program.sizeof(builder, elem_ir_type)
    program.call_extern(
        builder, "removeAt", [], [array, idx, elem_size], [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "W", "bits": 64}, 
            {"name": "W", "bits": 64}
        ], {"name": "Void"}
    )
    program.decr_ref(builder, elem, elem_type_)
    return elem, elem_type_


def insert(program, builder, params, param_types_):
    array, idx, value = params
    array_type_, _, value_type_ = param_types_
    elem_type_ = array_type_["generics"][0]
    casted_value = convert_type(program, builder, value, value_type_, elem_type_)
    elem_ir_type = make_type_(program, elem_type_)
    elem_size = program.sizeof(builder, elem_ir_type)
    program.call_extern(
        builder, "insertSpace", [], [array, idx, elem_size], [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "W", "bits": 64}, 
            {"name": "W", "bits": 64}
        ], {"name": "Void"}
    )
    content_ptr = builder.load(builder.gep(array, [i64_of(0), i32_of(3)]))
    content_ptr_casted = builder.bicast(content_ptr, elem_ir_type.as_pointer())
    builder.store(casted_value, builder.gep(content_ptr_casted, [idx]))
    return None, {"name": "Void"}


def clone(program, builder, params, param_types_):
    array, = params
    array_type_, = param_types_
    elem_type_ = array_type_["generics"][0]
    elem = progra.make_elem(builder, elem_type_)
    new_array = program.call_extern(
        builder, "clone", [array, elem], [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "W", "bits": 64, "generics": []}
        ], {"name": "Array", "bits": None, "generics": [
            {"name": "W", "bits": 8, "generics": []}
        ]}
    )
    return array, {"name": "Array", "bits": None, "generics": [
        {"name": "W", "bits": 8, "generics": []}
    ]}


def extend(program, builder, params, param_types_):
    array1, array2 = params
    array_type_, _ = param_types_ # array types_ will be the same
    elem_type_ = array_type_["generics"][0]
    elem = program.make_elem(builder, elem_type_)
    program.call_extern(
        builder, "extend", [array1, array2, elem], [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "W", "bits": 64, "generics": []}
        ], {"name": "Void"}
    )
    return None, {"name": "Void"}


def join(program, builder, params, param_types_):
    array1, array2 = params
    array_type_, _ = param_types_ # array types_ will be the same
    elem_type_ = array_type_["generics"][0]
    elem = program.make_elem(builder, elem_type_)
    return program.call_extern(
        builder, "join", [array1, array2, elem], [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "W", "bits": 64, "generics": []}
        ], array_type_
    ), array_type_


def make_range_array_1(program, builder, params, param_types_):
    end, = params
    return program.call_extern(
        builder, "rangeArray1", [end], [
            {"name": "Z", "bits": 32}
        ], {"name": "Array", "bits": None, "generics": [
            {"name": "W", "bits": 8, "generics": []}
        ]}
    ), {"name": "Array", "bits": None, "generics": [
        {"name": "W", "bits": 8, "generics": []}
    ]}


def make_range_array_2(program, builder, params, param_types_):
    start, end, = params
    return program.call_extern(
        builder, "rangeArray1", [start, end], [
            {"name": "Z", "bits": 32}, {"name": "Z", "bits": 32}
        ], {"name": "Array", "bits": None, "generics": [
            {"name": "W", "bits": 8, "generics": []}
        ]}
    ), {"name": "Array", "bits": None, "generics": [
        {"name": "W", "bits": 8, "generics": []}
    ]}


def make_range_array_3(program, builder, params, param_types_):
    start, end, step = params
    return program.call_extern(
        builder, "rangeArray1", [start, end, step], [
            {"name": "Z", "bits": 32}, {"name": "Z", "bits": 32},
            {"name": "Z", "bits": 32}
        ], {"name": "Array", "bits": None, "generics": [
            {"name": "W", "bits": 8, "generics": []}
        ]}
    ), {"name": "Array", "bits": None, "generics": [
        {"name": "W", "bits": 8, "generics": []}
    ]}


def abs_(program, builder, params, param_types_):
    value, = params
    return program.call_extern(
        builder, "abs", [value], [{"name": "Z", "bits": 32}],
        {"name": "W", "bits": 32}
    ), {"name": "W", "bits": 32}


def fabs(program, builder, params, param_types_):
    value, = params
    return program.call_extern(
        builder, "fabs", [value], [{"name": "Q", "bits": 64}],
        {"name": "Q", "bits": 64}
    ), {"name": "Q", "bits": 64}


BUILTINS = {
    -1: {
        "func": length,
        "params": [{"name": "Array", "bits": None, "generics": [
            {"name": "W", "bits": 8, "generics": []}
        ]}]
    },
    -2: {
        "func": capacity,
        "params": [{"name": "Array", "bits": None, "generics": [
            {"name": "W", "bits": 8, "generics": []}
        ]}]
    },
    -3: {
        "func": append,
        "params": [{"name": "Array", "bits": None, "generics": [
            {"name": "W", "bits": 8, "generics": []}
        ]}, None]
    },
    -4: {
        "func": require_capacity,
        "params": [{"name": "Array", "bits": None, "generics": [
            {"name": "W", "bits": 8, "generics": []}
        ]}, {"name": "W", "bits": 64, "generics": []}]
    },
    -5: {
        "func": shrink_mem,
        "params": [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}
        ]
    },
    -6: {
        "func": pop,
        "params": [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "W", "bits": 64, "generics": []}
        ]
    },
    -7: {
        "func": insert,
        "params": [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "W", "bits": 64, "generics": []},
            None
        ]
    },
    -8: {
        "func": clone,
        "params": [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}
        ]
    },
    -9: {
        "func": extend,
        "params": [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}
        ]
    },
    -10: {
        "func": join,
        "params": [
            {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}, {"name": "Array", "bits": None, "generics": [
                {"name": "W", "bits": 8, "generics": []}
            ]}
        ]
    },
    -11: {
        "func": make_range_array_1,
        "params": [
            {"name": "Z", "bits": 32}
        ]
    },
    -12: {
        "func": make_range_array_2,
        "params": [
            {"name": "Z", "bits": 32}, {"name": "Z", "bits": 32}
        ]
    },
    -13: {
        "func": make_range_array_3,
        "params": [
            {"name": "Z", "bits": 32}, {"name": "Z", "bits": 32},
            {"name": "Z", "bits": 32}
        ]
    },
    -14: {
        "func": abs_,
        "params": [{"name": "Z", "bits": 32}]
    },
    -15: {
        "func": fabs,
        "params": [{"name": "Q", "bits": 64}]
    }
}
