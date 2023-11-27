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
    }
}
