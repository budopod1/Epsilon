EXTERN_FUNCS = {
    "pow": {
        "name": "pow", "return_type_": {"name": "Q", "bits": 64}, 
        "arguments": [{"name": "Q", "bits": 64}, {"name": "Q", "bits": 64}]
    },
    "sqrt": {
        "name": "sqrt", "return_type_": {"name": "Q", "bits": 64}, 
        "arguments": [{"name": "Q", "bits": 64}]
    },
    "cbrt": {
        "name": "cbrt", "return_type_": {"name": "Q", "bits": 64}, 
        "arguments": [{"name": "Q", "bits": 64}]
    },
    "malloc": {
        "name": "malloc", "return_type_": {"name": "Pointer", "generics": [{"name": "W", "bits": 8}]},
        "arguments": [{"name": "W", "bits": 64}]
    },
    "free": {
        "name": "free", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Pointer", "generics": [{"name": "W", "bits": 8}]}]
    },
    "len": {
        "name": "len", "return_type_": {"name": "W", "bits": 64},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}]
    },
    "capacity": {
        "name": "capacity", "return_type_": {"name": "W", "bits": 64},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}]
    },
    "incrementLength": {
        "name": "incrementLength", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}]
    },
    "requireCapacity": {
        "name": "requireCapacity", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}, {"name": "W", "bits": 64}]
    },
    "shrinkMem": {
        "name": "shrinkMem", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}]
    },
    "removeAt": {
        "name": "removeAt", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}, {"name": "W", "bits": 64}]
    },
    "insertSpace": {
        "name": "insertSpace", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}, {"name": "W", "bits": 64}]
    },
    "incrementArrayRefCounts": {
        "name": "incrementArrayRefCounts", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}]
    },
    "clone": {
        "name": "clone", "return_type_": {"name": "Array", "generics": [{"name": "W", "bits": 8}]},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}]
    },
    "extend": {
        "name": "extend", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}]
    },
    "join": {
        "name": "join", "return_type_": {"name": "Array", "generics": [{"name": "W", "bits": 8}]},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}]
    },
    "rangeArray1": {
        "name": "rangeArray1", "return_type_": {"name": "Array", "generics": [{"name": "Z", "bits": 32}]},
        "arguments": [{"name": "Z", "bits": 32}]
    },
    "rangeArray2": {
        "name": "rangeArray1", "return_type_": {"name": "Array", "generics": [{"name": "Z", "bits": 32}]},
        "arguments": [{"name": "Z", "bits": 32}, {"name": "Z", "bits": 32}]
    },
    "rangeArray3": {
        "name": "rangeArray1", "return_type_": {"name": "Array", "generics": [{"name": "Z", "bits": 32}]},
        "arguments": [{"name": "Z", "bits": 32}, {"name": "Z", "bits": 32}, {"name": "Z", "bits": 32}]
    },
    "alwaysIncrementArrayRefCounts": {
        "name": "alwaysIncrementArrayRefCounts", "return_type_": {"name": "Void"},
        "arguments": [{"name": "Array", "generics": [{"name": "W", "bits": 8}]}, {"name": "W", "bits": 64}]
    },
    "abs": {
        "name": "abs", "return_type_": {"name": "W", "bits": 32},
        "arguments": [{"name": "Z", "bits": 32}]
    },
    "fabs": {
        "name": "fabs", "return_type_": {"name": "Q", "bits": 64},
        "arguments": [{"name": "Q", "bits": 64}]
    }
}

EXTERN_ARRAYS = [
    {"name": "Array", "generics": [{"name": "W", "bits": 8}]},
    {"name": "Array", "generics": [{"name": "Z", "bits": 32}]}
]
