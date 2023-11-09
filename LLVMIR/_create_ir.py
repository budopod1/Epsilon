import llvmlite
import orjson


def main():
    with open("temp.json") as file:
        data = orjson.loads(file.read())
    print(data)
