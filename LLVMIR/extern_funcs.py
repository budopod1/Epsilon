from common import *


EXTERN_FUNCS = {
    "pow": {
        "name": "llvm.pow.f64", "return_type_": Q64, 
        "arguments": [Q64, Q64]
    },
    "sqrt": {
        "name": "llvm.sqrt.f64", "return_type_": Q64, 
        "arguments": [Q64]
    },
    "cbrt": {
        "name": "llvm.cbrt.f64", "return_type_": Q64, 
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
    "concat": {
        "name": "concat", "return_type_": ArrayW8,
        "arguments": [ArrayW8, ArrayW8, W64]
    },
    "rangeArray1": {
        "name": "rangeArray1", "return_type_": ArrayZ32,
        "arguments": [Z32]
    },
    "rangeArray2": {
        "name": "rangeArray2", "return_type_": ArrayZ32,
        "arguments": [Z32, Z32]
    },
    "rangeArray3": {
        "name": "rangeArray3", "return_type_": ArrayZ32,
        "arguments": [Z32, Z32, Z32]
    },
    "abs": {
        "name": "llvm.abs.i32", "return_type_": W32,
        "arguments": [Z32, Bool]
    },
    "fabs": {
        "name": "llvm.fabs.i32", "return_type_": Q64,
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
        "name": "llvm.memcpy.p0.p0.i64", "return_type_": VOID,
        "arguments": [PointerW8, PointerW8, W64, Bool]
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
        "name": "slice", "return_type_": ArrayW8,
        "arguments": [ArrayW8, W64, W64, W64]
    },
    "arrayEqual": {
        "name": "arrayEqual", "return_type_": Z32,
        "arguments": [ArrayW8, ArrayW8], "bool_ret": True
    },
    "countChr": {
        "name": "countChr", "return_type_": W64,
        "arguments": [String, Byte]
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
    },
    "startsWith": {
        "name": "startWith", "return_type_": Z32,
        "arguments": [ArrayW8, ArrayW8, W64], "bool_ret": True
    },
    "endsWith": {
        "name": "endsWith", "return_type_": Z32,
        "arguments": [ArrayW8, ArrayW8, W64], "bool_ret": True
    },
    "arrayEqual": {
        "name": "arrayEqual", "return_type_": Z32,
        "arguments": [ArrayW8, ArrayW8, W64], "bool_ret": True
    },
    "memcmp": {
        "name": "memcmp", "return_type_": Z32,
        "arguments": [PointerW8, PointerW8, W64]
    },
    "join": {
        "name": "join", "return_type_": ArrayW8,
        "arguments": [ArrayW8, ArrayW8, W64]
    },
    "indexOfSubsection": {
        "name": "indexOfSubsection", "return_type_": Z64,
        "arguments": [ArrayW8, ArrayW8, W64]
    },
    "parseInt": {
        "name": "parseInt", "return_type_": Z32,
        "arguments": [String]
    },
    "isValidParsedInt": {
        "name": "isValidParsedInt", "return_type_": Z32,
        "arguments": [Z32], "bool_ret": True
    },
    "parseFloat": {
        "name": "parseFloat", "return_type_": Q32,
        "arguments": [String]
    },
    "isValidParsedFloat": {
        "name": "isValidParsedFloat", "return_type_": Z32,
        "arguments": [Q32], "bool_ret": True
    },
    "readInputLine": {
        "name": "readInputLine", "return_type_": String,
        "arguments": []
    },
    "openFile": {
        "name": "openFile", "return_type_": File,
        "arguments": [String, Z32]
    },
    "FILE_READ_MODE": {
        "name": "FILE_READ_MODE", "return_type_": Z32,
        "arguments": []
    },
    "FILE_WRITE_MODE": {
        "name": "FILE_WRITE_MODE", "return_type_": Z32,
        "arguments": []
    },
    "FILE_APPEND_MODE": {
        "name": "FILE_APPEND_MODE", "return_type_": Z32,
        "arguments": []
    },
    "FILE_BINARY_MODE": {
        "name": "FILE_BINARY_MODE", "return_type_": Z32,
        "arguments": []
    },
    "fileOpen": {
        "name": "fileOpen", "return_type_": Z32,
        "arguments": [File], "bool_ret": True
    },
    "fileMode": {
        "name": "fileMode", "return_type_": Z32,
        "arguments": [File]
    },
    "closeFile": {
        "name": "closeFile", "return_type_": Z32,
        "arguments": [File], "bool_ret": True
    },
    "fileLength": {
        "name": "fileLength", "return_type_": Z64,
        "arguments": [File]
    },
    "filePos": {
        "name": "filePos", "return_type_": Z64,
        "arguments": [File]
    },
    "readAllFile": {
        "name": "readAllFile", "return_type_": OptionalString,
        "arguments": [File]
    },
    "readSomeFile": {
        "name": "readSomeFile", "return_type_": OptionalString,
        "arguments": [File, W64]
    },
    "setFilePos": {
        "name": "setFilePos", "return_type_": Z32,
        "arguments": [File, W64], "bool_ret": True
    },
    "jumpFilePos": {
        "name": "jumpFilePos", "return_type_": Z32,
        "arguments": [File, W64], "bool_ret": True
    },
    "readFileLine": {
        "name": "readFileLine", "return_type_": OptionalString,
        "arguments": [File]
    },
    "readLineReachedEOF": {
        "name": "readLineReachedEOF", "return_type_": Z32,
        "arguments": [], "bool_ret": True
    },
    "readFileLines": {
        "name": "readFileLines", "return_type_": OptionalArrayString,
        "arguments": [File]
    },
    "writeToFile": {
        "name": "writeToFile", "return_type_": Z32,
        "arguments": [File, String], "bool_ret": True
    },
    "freeFile": {
        "name": "freeFile", "return_type_": VOID,
        "arguments": [File]
    },
    "abort_": {
        "name": "abort_", "return_type_": VOID,
        "arguments": [String]
    },
    "makeBlankArray": {
        "name": "makeBlankArray", "return_type_": ArrayW8,
        "arguments": [W64, W64]
    },
}

EXTERN_ARRAYS = [
    ArrayW8, ArrayZ32, String, ArrayString
]
