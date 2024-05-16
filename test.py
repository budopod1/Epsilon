import llvmlite.binding as llvm
from ctypes import CFUNCTYPE, c_double, c_float, c_int, c_uint, c_ushort, c_ulong, c_long, c_char
import subprocess
from pathlib import Path
import sys
import multiprocessing


engine = None
manager = multiprocessing.Manager()


TESTS = [
    {
        "file": "basic.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "if3.epsl",
        "func": -1,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 0}
        ]
    },
    {
        "file": "if2.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 1},
            {"arguments": [2], "compare": "exact", "expect": 0}
        ]
    },
    {
        "file": "multipath.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 2},
            {"arguments": [3], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "file.epsl",
        "func": -1,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 20}
        ]
    },
    {
        "file": "mathtest.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_double, c_double),
        "tests": [
            {"arguments": [3], "compare": "float", "expect": 35.54}
        ]
    },
    {
        "file": "if.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [6], "compare": "exact", "expect": 1},
            {"arguments": [5], "compare": "exact", "expect": 0},
            {"arguments": [4], "compare": "exact", "expect": -1}
        ]
    },
    {
        "file": "while.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [4], "compare": "exact", "expect": 10},
            {"arguments": [6], "compare": "exact", "expect": 21},
        ]
    },
    {
        "file": "string.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_char, c_int),
        "tests": [
            {"arguments": [0], "compare": "exact", "expect": b"a"},
            {"arguments": [14], "compare": "exact", "expect": b"o"},
        ]
    },
    {
        "file": "array.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int, c_int),
        "tests": [
            {"arguments": [0, 0], "compare": "exact", "expect": 1},
            {"arguments": [2, 1], "compare": "exact", "expect": 6},
        ]
    },
    {
        "file": "struct.epsl",
        "func": -1,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 5}
        ]
    },
    {
        "file": "switch.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 2},
            {"arguments": [2], "compare": "exact", "expect": -2},
            {"arguments": [3], "compare": "exact", "expect": 5},
        ]
    },
    {
        "file": "builtin.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [4], "compare": "exact", "expect": 2},
            {"arguments": [5], "compare": "exact", "expect": 6},
            {"arguments": [17], "compare": "exact", "expect": 72},
        ]
    },
    {
        "file": "stringify.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [10], "compare": "exact", "expect": 2851}
        ]
    },
    {
        "file": "equals.epsl",
        "func": -1,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 2}
        ]
    },
    {
        "file": "bool.epsl",
        "func": -1,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "deepequals.epsl",
        "func": -1,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 10}
        ]
    },
    {
        "file": "compound.epsl",
        "func": -1,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 14}
        ]
    },
    {
        "file": "uninitvalue.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 42},
            {"arguments": [0], "compare": "exact", "expect": 24}
        ]
    },
    {
        "file": "circular1.epsl",
        "func": -1,
        "sig": CFUNCTYPE(c_int),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 2}
        ]
    },
    {
        "file": "mathimport.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_double, c_int),
        "tests": [
            {"arguments": [1], "compare": "float", "expect": 0.54}
        ]
    },
    {
        "file": "mathimport3.epsl",
        "func": 0,
        "sig": CFUNCTYPE(c_double, c_double),
        "tests": [
            {"arguments": [-4], "compare": "float", "expect": 2}
        ]
    }
]

TIMEOUT = 15


def get_func(path, source, func_id):
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

    func = "main" if func_id == -1 else f"{str(source.resolve())}/{str(func_id)}"

    return engine.get_function_address(func)


def compile_file(file):
    proccess = subprocess.run(
        ["mono", "Epsilon.exe", "-t", "llvm-ll", "compile", str(file), "-o", "code.ll", "-P"],
        capture_output=True, timeout=TIMEOUT
    )
    output = proccess.stdout+proccess.stderr
    if proccess.returncode:
        return False, output.decode('utf-8')
    return True, ""


def equal(mode, a, b):
    if mode == "exact":
        return a == b
    elif mode == "float":
        return abs(a - b) < 0.005


test_result = manager.dict()


def run_test(func, args):
    test_result["result"] = func(*args)


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
        source = base_dir/file

        try:
            did_compile, compile_message = compile_file(source)
        except subprocess.TimeoutExpired:
            print("❗ Compilation of function failed:")
            print(f"Compliation did not complete within {TIMEOUT} seconds")
            failed += len(group["tests"])
            continue

        if not did_compile:
            print("❗ Compilation of function failed with error:")
            print(compile_message)
            failed += len(group["tests"])
            continue
        
        func_ptr = get_func("code.ll", source.with_suffix(""), func)
        func = group["sig"](func_ptr)
        
        for test in group["tests"]:
            print(f"Running test {i}/{test_count}...")

            test_result["result"] = None
            
            process = multiprocessing.Process(
                target=run_test, args=(func, test["arguments"]))
            process.start()
            process.join(TIMEOUT)

            if process.is_alive():
                process.terminate()
                process.join()
                print("❌ Test failed!")
                print(f"Test did not complete within {TIMEOUT} seconds")
                failed += 1
            else:
                expect = test["expect"]

                result = test_result["result"]

                if result is None:
                    print("😬 Probable segmentation fault :(")
                    failed += 1
                elif equal(test["compare"], result, expect):
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
