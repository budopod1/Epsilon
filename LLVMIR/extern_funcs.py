from common import *


EXTERN_FUNCS = {
    "pow": {
        "name": "pow", "return_type_": Q64, 
        "arguments": [Q64, Q64]
    },
    "sqrt": {
        "name": "sqrt", "return_type_": Q64, 
        "arguments": [Q64]
    },
    "cbrt": {
        "name": "cbrt", "return_type_": Q64, 
        "arguments": [Q64]
    },
    "malloc": {
        "name": "malloc", "return_type_": PointerW8,
        "arguments": [W64]
    },
    "free": {
        "name": "free", "return_type_": VOID,
        "arguments": [PointerW8]
    },
    "len": {
        "name": "len", "return_type_": W64,
        "arguments": [ArrayW8]
    },
    "capacity": {
        "name": "capacity", "return_type_": W64,
        "arguments": [ArrayW8]
    },
    "incrementLength": {
        "name": "incrementLength", "return_type_": VOID,
        "arguments": [ArrayW8, W64]
    },
    "requireCapacity": {
        "name": "requireCapacity", "return_type_": VOID,
        "arguments": [ArrayW8, W64, W64]
    },
    "shrinkMem": {
        "name": "shrinkMem", "return_type_": VOID,
        "arguments": [ArrayW8, W64]
    },
    "removeAt": {
        "name": "removeAt", "return_type_": VOID,
        "arguments": [ArrayW8, W64, W64]
    },
    "insertSpace": {
        "name": "insertSpace", "return_type_": VOID,
        "arguments": [ArrayW8, W64, W64]
    },
    "incrementArrayRefCounts": {
        "name": "incrementArrayRefCounts", "return_type_": VOID,
        "arguments": [ArrayW8, W64]
    },
    "clone": {
        "name": "clone", "return_type_": ArrayW8,
        "arguments": [ArrayW8, W64]
    },
    "extend": {
        "name": "extend", "return_type_": VOID,
        "arguments": [ArrayW8, ArrayW8, W64]
    },
    "join": {
        "name": "join", "return_type_": ArrayW8,
        "arguments": [ArrayW8, ArrayW8, W64]
    },
    "rangeArray1": {
        "name": "rangeArray1", "return_type_": ArrayZ32,
        "arguments": [Z32]
    },
    "rangeArray2": {
        "name": "rangeArray1", "return_type_": ArrayZ32,
        "arguments": [Z32, Z32]
    },
    "rangeArray3": {
        "name": "rangeArray1", "return_type_": ArrayZ32,
        "arguments": [Z32, Z32, Z32]
    },
    "alwaysIncrementArrayRefCounts": {
        "name": "alwaysIncrementArrayRefCounts", "return_type_": VOID,
        "arguments": [ArrayW8, W64]
    },
    "abs": {
        "name": "abs", "return_type_": W32,
        "arguments": [Z32]
    },
    "fabs": {
        "name": "fabs", "return_type_": Q64,
        "arguments": [Q64]
    },
    "print": {
        "name": "print", "return_type_": VOID,
        "arguments": [String]
    },
    "println": {
        "name": "println", "return_type_": VOID,
        "arguments": [String]
    },
    "snprintf": {
        "name": "snprintf", "return_type_": W64,
        "arguments": [PointerW8, W64, PointerW8],
        "vargs": True
    },
    "realloc": {
        "name": "realloc", "return_type_": PointerW8,
        "arguments": [PointerW8, W64]
    },
    "memcpy": {
        "name": "memcpy", "return_type_": VOID,
        "arguments": [PointerW8, PointerW8, W64]
    },
    "sprintf": {
        "name": "sprintf", "return_type_": W64,
        "arguments": [PointerW8, PointerW8],
        "vargs": True
    },
    "formatW8": {
        "name": "formatW8", "return_type_": PointerW8, "arguments": []
    },
    "formatW16": {
        "name": "formatW16", "return_type_": PointerW8, "arguments": []
    },
    "formatW32": {
        "name": "formatW32", "return_type_": PointerW8, "arguments": []
    },
    "formatW64": {
        "name": "formatW64", "return_type_": PointerW8, "arguments": []
    },
    "formatZ8": {
        "name": "formatZ8", "return_type_": PointerW8, "arguments": []
    },
    "formatZ16": {
        "name": "formatZ16", "return_type_": PointerW8, "arguments": []
    },
    "formatZ32": {
        "name": "formatZ32", "return_type_": PointerW8, "arguments": []
    },
    "formatZ64": {
        "name": "formatZ64", "return_type_": PointerW8, "arguments": []
    },
    "leftPad": {
        "name": "leftPad", "return_type_": VOID, 
        "arguments": [String, W64, Byte]
    },
    "rightPad": {
        "name": "rightPad", "return_type_": VOID, 
        "arguments": [String, W64, Byte]
    },
    "slice": {
        "name": "slice", "return_type_": VOID,
        "arguments": [ArrayW8, W64, W64, W64]
    },
    "arrayEqual": {
        "name": "arrayEqual", "return_type_": Z32,
        "arguments": [ArrayW8, ArrayW8]
    },
    "countChr": {
        "name": "countChr", "return_type_": W64,
        "arguments": [ArrayW8, Byte]
    },
    "count": {
        "name": "count", "return_type_": W64,
        "arguments": [ArrayW8, ArrayW8, W64]
    },
    "overlapCount": {
        "name": "overlapCount", "return_type_": W64,
        "arguments": [ArrayW8, ArrayW8, W64]
    },
    "nest": {
        "name": "nest", "return_type_": ArrayW8,
        "arguments": [ArrayW8, W64]
    },
    "split": {
        "name": "split", "return_type_": ArrayW8,
        "arguments": [ArrayW8, ArrayW8, W64]
    }
}

EXTERN_ARRAYS = [
    ArrayW8, ArrayZ32, String
]
