#!/usr/bin/env python3

import os
import queue
import subprocess
import multiprocessing
from pathlib import Path
from ctypes import c_double, c_int32, c_uint64, c_char, CDLL


TESTS = [
    {
        "file": Path("Annotations") / "entry.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 7}
        ]
    },
    {
        "file": Path("CPP") / "entry.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 4}
        ]
    },
    {
        "file": Path("Internal") / "entry.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 12}
        ]
    },
    {
        "file": "affix.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 25}
        ]
    },
    {
        "file": "argsort.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 25}
        ]
    },
    {
        "file": "arithmetic.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 51}
        ]
    },
    {
        "file": "array.epsl",
        "func": "testee",
        "sig": (c_int32, c_int32, c_int32),
        "tests": [
            {"arguments": [0, 0], "compare": "exact", "expect": 1},
            {"arguments": [2, 1], "compare": "exact", "expect": 6},
        ]
    },
    {
        "file": "arrayaccess.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 16}
        ]
    },
    {
        "file": "arrayassign.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 7}
        ]
    },
    {
        "file": "at.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 33}
        ]
    },
    {
        "file": "basic.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "bitshift.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 97}
        ]
    },
    {
        "file": "bitwise.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "blankarray.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 4}
        ]
    },
    {
        "file": "bool.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "capacity.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 0}
        ]
    },
    {
        "file": "circular1.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 2}
        ]
    },
    {
        "file": "compound.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 10}
        ]
    },
    {
        "file": "count.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 36}
        ]
    },
    {
        "file": "dedup.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 8709120}
        ]
    },
    {
        "file": "equals.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 10}
        ]
    },
    {
        "file": "file.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 20}
        ]
    },
    {
        "file": "format.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 352},
        ]
    },
    {
        "file": "for.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 53},
        ]
    },
    {
        "file": "given.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 230}
        ]
    },
    {
        "file": "global.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "if.epsl",
        "func": "testee",
        "sig": (c_int32, c_int32),
        "tests": [
            {"arguments": [6], "compare": "exact", "expect": 1},
            {"arguments": [5], "compare": "exact", "expect": 0},
            {"arguments": [4], "compare": "exact", "expect": -1}
        ]
    },
    {
        "file": "if2.epsl",
        "func": "testee",
        "sig": (c_int32, c_int32),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 1},
            {"arguments": [2], "compare": "exact", "expect": 0}
        ]
    },
    {
        "file": "if3.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 0}
        ]
    },
    {
        "file": "indexof.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 5}
        ]
    },
    {
        "file": "join.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 4271}
        ]
    },
    {
        "file": "insert.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 676}
        ]
    },
    {
        "file": "mathimport.epsl",
        "func": "testee",
        "sig": (c_double, c_int32),
        "tests": [
            {"arguments": [1], "compare": "float", "expect": 0.54}
        ]
    },
    {
        "file": "mathimport3.epsl",
        "func": "testee",
        "sig": (c_double, c_double),
        "tests": [
            {"arguments": [-4], "compare": "float", "expect": 2}
        ]
    },
    {
        "file": "mathtest.epsl",
        "func": "testee",
        "sig": (c_double, c_double),
        "tests": [
            {"arguments": [3], "compare": "float", "expect": 35.54}
        ]
    },
    {
        "file": "multipath.epsl",
        "func": "testee",
        "sig": (c_int32, c_int32),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 2},
            {"arguments": [3], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "nestedoptional.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 3}
        ]
    },
    {
        "file": "nullkw.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "pad.epsl",
        "func": "testee",
        "sig": (c_uint64,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 10391070184567263232}
        ]
    },
    {
        "file": "parsenum.epsl",
        "func": "testee",
        "sig": (c_double,),
        "tests": [
            {"arguments": [], "compare": "float", "expect": 586.789}
        ]
    },
    {
        "file": "poly.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 15}
        ]
    },
    {
        "file": "pop.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 7}
        ]
    },
    {
        "file": "repeat.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 78}
        ]
    },
    {
        "file": "round.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1}
        ]
    },
    {
        "file": "shortcircuit.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 22}
        ]
    },
    {
        "file": "slice.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 507}
        ]
    },
    {
        "file": "sort.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 564}
        ]
    },
    {
        "file": "split.epsl",
        "func": "testee",
        "sig": (c_uint64,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 4359359347201}
        ]
    },
    {
        "file": "startsends.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 6}
        ]
    },
    {
        "file": "string.epsl",
        "func": "testee",
        "sig": (c_char, c_int32),
        "tests": [
            {"arguments": [0], "compare": "exact", "expect": b"a"},
            {"arguments": [14], "compare": "exact", "expect": b"o"},
        ]
    },
    {
        "file": "stringify.epsl",
        "func": "testee",
        "sig": (c_int32, c_int32),
        "tests": [
            {"arguments": [10], "compare": "exact", "expect": 2851}
        ]
    },
    {
        "file": "struct.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 5}
        ]
    },
    {
        "file": "structtype.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 41}
        ]
    },
    {
        "file": "switch.epsl",
        "func": "testee",
        "sig": (c_int32, c_int32),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 2},
            {"arguments": [2], "compare": "exact", "expect": -2},
            {"arguments": [3], "compare": "exact", "expect": 5},
        ]
    },
    {
        "file": "switchstr.epsl",
        "func": "testee",
        "sig": (c_int32, c_int32),
        "tests": [
            {"arguments": [1], "compare": "exact", "expect": 10},
            {"arguments": [10], "compare": "exact", "expect": 8},
            {"arguments": [20], "compare": "exact", "expect": 0},
        ]
    },
    {
        "file": "unique.epsl",
        "func": "main",
        "sig": (c_int32,),
        "tests": [
            {"arguments": [], "compare": "exact", "expect": 1080}
        ]
    },
    {
        "file": "while.epsl",
        "func": "testee",
        "sig": (c_int32, c_int32),
        "tests": [
            {"arguments": [4], "compare": "exact", "expect": 10},
            {"arguments": [6], "compare": "exact", "expect": 21},
        ]
    }
]


TIMEOUT = 15


def compile_file(source, result_file):
    proccess = subprocess.run(
        [
            "dotnet", "executables/Epsilon.dll", "compile", "-t",
            "shared-object", str(source), "-o", str(result_file), "-H",
            "dont-use",
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


def do_test_proc(comm, result_file, func_name, sig, args):
    so = CDLL(result_file)
    func = getattr(so, func_name)
    func.restype, *func.argtypes = sig
    comm.put(func(*args))


def nowait_queue_get(queue_obj):
    try:
        return queue_obj.get_nowait()
    except queue.Empty:
        return None


def main():
    os.chdir(Path(__file__).parents[1].absolute())

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
            test_idx += len(group["tests"])
            continue

        if not did_compile:
            print("❗ Compilation of function failed with error:")
            print(compile_message)
            failed += len(group["tests"])
            test_idx += len(group["tests"])
            continue

        for test in group["tests"]:
            print(f"Running test {test_idx}/{test_count}...")

            comm = multiprocessing.Queue()

            process = multiprocessing.Process(target=do_test_proc, args=(
                comm, result_file, group["func"], group["sig"], test["arguments"]
            ))
            process.start()
            process.join(TIMEOUT)
            process.terminate()
            process.join()

            if process.is_alive():
                print("❌ Test failed!")
                print(f"Test did not complete within {TIMEOUT} seconds")
                failed += 1
            else:
                result = nowait_queue_get(comm)

                expect = test["expect"]

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

        try:
            exp_file = result_file.with_suffix(".exp")
            if exp_file.exists():
                exp_file.unlink()
            lib_file = result_file.with_suffix(".lib")
            if lib_file.exists():
                lib_file.unlink()
            result_file.unlink()
        except PermissionError:
            pass

    print("\nTests completed:")
    if failed > 0:
        print(f"{failed}/{test_count} failed")
    print(f"{succeeded}/{test_count} succeeded")
    if succeeded == test_count:
        print("All tests passed 🥳!")


if __name__ == "__main__":
    main()
