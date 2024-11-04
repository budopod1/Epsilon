import os
import time
import subprocess
import multiprocessing
from pathlib import Path
from ctypes import c_double, c_float, c_int, c_uint, c_ushort, c_ulong, c_long, c_char, CDLL


TESTS = [
    {
        "file": Path("Annotations") / "entry.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 7}
        ]
    },
    {
        "file": Path("CPP") / "entry.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 4}
        ]
    },
    {
        "file": Path("Internal") / "entry.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 12}
        ]
    },
    {
        "file": "arithmetic.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 51}
        ]
    },
    {
        "file": "array.epsl",
        "func": 0,
        "sig": (c_int, c_int, c_int),
        "tests": [
            {"arguments": [0, 0], "compare": "exact", "expect": 1},
            {"arguments": [2, 1], "compare": "exact", "expect": 6},
        ]
    },
    {
        "file": "arrayaccess.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 16}
        ]
    },
    {
        "file": "arrayassign.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 7}
        ]
    },
    {
        "file": "basic.epsl",
        "func": 0,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "bitshift.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 97}
        ]
    },
    {
        "file": "bitwise.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "blankarray.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "bool.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "capacity.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 0}
        ]
    },
    {
        "file": "circular1.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 2}
        ]
    },
    {
        "file": "compound.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 10}
        ]
    },
    {
        "file": "count.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 36}
        ]
    },
    {
        "file": "equals.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 10}
        ]
    },
    {
        "file": "file.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 20}
        ]
    },
    {
        "file": "format.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 352},
        ]
    },
    {
        "file": "for.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 49},
        ]
    },
    {
        "file": "given.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 230}
        ]
    },
    {
        "file": "global.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "if.epsl",
        "func": 0,
        "sig": (c_int, c_int),
        "tests": [
            {"arguments": [6], "compare": "exact", "expect": 1},
            {"arguments": [5], "compare": "exact", "expect": 0},
            {"arguments": [4], "compare": "exact", "expect": -1}
        ]
    },
    {
        "file": "if2.epsl",
        "func": 0,
        "sig": (c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 1},
            {"arguments": [2], "compare": "exact", "expect": 0}
        ]
    },
    {
        "file": "if3.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 0}
        ]
    },
    {
        "file": "increment.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 6}
        ]
    },
    {
        "file": "indexof.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 5}
        ]
    },
    {
        "file": "join.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 4271}
        ]
    },
    {
        "file": "insert.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 676}
        ]
    },
    {
        "file": "mathimport.epsl",
        "func": 0,
        "sig": (c_double, c_int),
        "tests": [
            {"arguments": [1], "compare": "float", "expect": 0.54}
        ]
    },
    {
        "file": "mathimport3.epsl",
        "func": 0,
        "sig": (c_double, c_double),
        "tests": [
            {"arguments": [-4], "compare": "float", "expect": 2}
        ]
    },
    {
        "file": "mathtest.epsl",
        "func": 0,
        "sig": (c_double, c_double),
        "tests": [
            {"arguments": [3], "compare": "float", "expect": 35.54}
        ]
    },
    {
        "file": "multipath.epsl",
        "func": 0,
        "sig": (c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 2},
            {"arguments": [3], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "nullkw.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "pad.epsl",
        "func": 0,
        "sig": (c_ulong,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 10391070184567263232}
        ]
    },
    {
        "file": "parsenum.epsl",
        "func": 0,
        "sig": (c_double,),
        "tests": [
            {"arguments": [], "compare": "float", "expect": 581.789}
        ]
    },
    {
        "file": "pop.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 7}
        ]
    },
    {
        "file": "shortcircuit.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 22}
        ]
    },
    {
        "file": "split.epsl",
        "func": 0,
        "sig": (c_ulong,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 4359359347200}
        ]
    },
    {
        "file": "startsends.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 6}
        ]
    },
    {
        "file": "string.epsl",
        "func": 0,
        "sig": (c_char, c_int),
        "tests": [
            {"arguments": [0], "compare": "exact", "expect": b"a"},
            {"arguments": [14], "compare": "exact", "expect": b"o"},
        ]
    },
    {
        "file": "stringify.epsl",
        "func": 0,
        "sig": (c_int, c_int),
        "tests": [
            {"arguments": [10], "compare": "exact", "expect": 2851}
        ]
    },
    {
        "file": "struct.epsl",
        "func": -1,
        "sig": (c_int,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 5}
        ]
    },
    {
        "file": "switch.epsl",
        "func": 0,
        "sig": (c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 2},
            {"arguments": [2], "compare": "exact", "expect": -2},
            {"arguments": [3], "compare": "exact", "expect": 5},
        ]
    },
    {
        "file": "uninitvalue.epsl",
        "func": 0,
        "sig": (c_int, c_int),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 42},
            {"arguments": [0], "compare": "exact", "expect": 24}
        ]
    },
    {
        "file": "while.epsl",
        "func": 0,
        "sig": (c_int, c_int),
        "tests": [
            {"arguments": [4], "compare": "exact", "expect": 10},
            {"arguments": [6], "compare": "exact", "expect": 21},
        ]
    }
]


manager = multiprocessing.Manager()


TIMEOUT = 15


def get_func(func_id, source, result_file):
    so = CDLL(result_file)

    func = "main" if func_id == -1 else f"{source.with_suffix('')}/{str(func_id)}"

    return getattr(so, func)


def compile_file(source, result_file):
    proccess = subprocess.run(
        [
            "./scripts/epslc.bash", "compile", "-t", "shared-object",
            str(source), "-o", str(result_file), "-H", "dont-use",
        ], capture_output=True, timeout=TIMEOUT
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
    base_dir = Path("tests").resolve()

    print("🔬 Running tests...")

    test_count = sum(map(lambda v: len(v["tests"]), TESTS))
    test_idx = 1
    succeeded = 0
    failed = 0

    for file_idx, group in enumerate(TESTS):
        filename = group["file"]
        print(f"\n🧪 Testing file {filename}...")

        source = base_dir/filename
        result_file = base_dir/f"o{file_idx}.so"

        try:
            did_compile, compile_message = compile_file(source, result_file)
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

        func = get_func(group["func"], source, result_file)
        func.restype, *func.argtypes = group["sig"]

        for test in group["tests"]:
            print(f"Running test {test_idx}/{test_count}...")

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

            test_idx += 1

        os.remove(result_file)

    print("\nTests completed:")
    if failed > 0:
        print(f"{failed}/{test_count} failed")
    print(f"{succeeded}/{test_count} succeeded")
    if succeeded == test_count:
        print("All tests passed 🥳!")


if __name__ == "__main__":
    main()
