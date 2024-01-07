import llvmlite.binding as llvm
from ctypes import CFUNCTYPE, c_double, c_float, c_int, c_uint, c_ushort, c_ulong, c_long, c_char
import subprocess
from pathlib import Path
import sys


engine = None


TESTS = [
    {
        "file": "basic.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "math.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_float, c_float),
        "tests": [
            {"arguments": [3], "compare": "float", "expect": 35.54}
        ]
    },
    {
        "file": "if.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [6], "compare": "exact", "expect": 1},
            {"arguments": [5], "compare": "exact", "expect": 0},
            {"arguments": [4], "compare": "exact", "expect": -1}
        ]
    },
    {
        "file": "while.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [4], "compare": "exact", "expect": 10},
            {"arguments": [6], "compare": "exact", "expect": 21},
        ]
    },
    {
        "file": "string.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_char, c_int),
        "tests": [
            {"arguments": [0], "compare": "exact", "expect": b"a"},
            {"arguments": [14], "compare": "exact", "expect": b"o"},
        ]
    },
    {
        "file": "array.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_int, c_int, c_int),
        "tests": [
            {"arguments": [0, 0], "compare": "exact", "expect": 1},
            {"arguments": [2, 1], "compare": "exact", "expect": 6},
        ]
    },
    {
        "file": "struct.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [0], "compare": "exact", "expect": 5},
            {"arguments": [1], "compare": "exact", "expect": 3},
        ]
    },
    {
        "file": "switch.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 2},
            {"arguments": [2], "compare": "exact", "expect": -2},
            {"arguments": [3], "compare": "exact", "expect": 5},
        ]
    },
    {
        "file": "builtin.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [4], "compare": "exact", "expect": 2},
            {"arguments": [5], "compare": "exact", "expect": 6},
            {"arguments": [17], "compare": "exact", "expect": 72},
        ]
    },
    {
        "file": "stringify.epsl",
        "func": "func0",
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [10], "compare": "exact", "expect": 2851}
        ]
    },
    {
        "file": "equals.epsl",
        "func": "main",
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 2}
        ]
    },
    {
        "file": "bool.epsl",
        "func": "main",
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "deepequals.epsl",
        "func": "main",
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 10}
        ]
    },
    {
        "file": "compound.epsl",
        "func": "main",
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 14}
        ]
    },
]


def get_func(path, func):
    global engine
    
    with open(path) as file:
        module = file.read()

    llvm.initialize()
    llvm.initialize_native_target()
    llvm.initialize_native_asmprinter()

    target = llvm.Target.from_default_triple()
    target_machine = target.create_target_machine()
    
    backing_mod = llvm.parse_assembly("")
    engine = llvm.create_mcjit_compiler(backing_mod, target_machine)
    
    module = llvm.parse_assembly(module)
    module.verify()
    engine.add_module(module)
    engine.finalize_object()
    engine.run_static_constructors()

    return engine.get_function_address(func)


def compile_file(file):
    proccess = subprocess.run(
        ["mono", "Epsilon.exe", "-w", "compile", str(file), "_"], capture_output=True
    )
    error_markers = [b"compilation error", b"Error in", b"Unhandled Exception"]
    for error_marker in error_markers:
        if error_marker in error_markers:
            return False, (
                proccess.stdout.decode('utf-8')+proccess.stderr.decode('utf-8')
            )
    subprocess.run(["llvm-dis", "-o", "code-opt.ll", "code-opt.bc"])
    return True, ""


def equal(mode, a, b):
    if mode == "exact":
        return a == b
    elif mode == "float":
        return round(a, 2) == round(b, 2)


def main():
    base_dir = Path("examples")
    
    print("🔬 Running tests...")

    test_count = sum(map(lambda v: len(v["tests"]), TESTS))
    i = 1
    succeeded = 0
    failed = 0
    
    for group in TESTS:
        file = group["file"]
        func = group["func"]
        print(f"\n🧪 Testing function {func} from file {file}...")
        compiled, msg = compile_file(base_dir/file)

        if not compiled:
            print("❗ Compilation of function failed with error:")
            print(msg)
            failed += len(group["tests"])
            continue
        
        func_ptr = get_func("code-opt.ll", func)
        func = group["sig"](func_ptr)
        
        for test in group["tests"]:
            print(f"Running test {i}/{test_count}...")
            result = func(*test["arguments"])
            expect = test["expect"]
            
            if equal(test["compare"], result, expect):
                print("✅ Test passed")
                succeeded += 1
            else:
                print("❌ Test failed!")
                print(f"Expected {expect}, got {result}")
                failed += 1

            i += 1
    
    print("\nTests completed:")
    if failed > 0:
        print(f"{failed}/{test_count} failed")
    print(f"{succeeded}/{test_count} succeeded")
    if succeeded == test_count:
        print("All tests passed 🥳!")


if __name__ == "__main__":
    main()
