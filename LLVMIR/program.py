class Program:
    def __init__(self, module):
        self.module = module
        self.functions = {}
        self.structs = {}
        self.array_ids = {}
        self.arrays = {}

    def add_function(self, function):
        self.functions[function.id_] = function

    def add_struct(self, struct):
        self.structs[struct.name] = struct

    def add_array(self, array):
        self.arrays[array.id_] = array
