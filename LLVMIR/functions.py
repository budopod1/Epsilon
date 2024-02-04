from llvmlite import ir
from common import *
from blocks import Block
from instructions import BaseInstruction, FlowInstruction


class Function:
    def __init__(self, program, id_, data):
        self.program = program
        self.id_ = id_
        self.return_type_= data["return_type_"]
        self.arguments = data["arguments"]
        self.is_main = data["is_main"]
        self.declarations = data["declarations"]
        self.special_alloc_types_ = data["special_allocs"]
        self.callee = data["callee"]
        self.ir_type = make_function_type_(
            program, self.return_type_, 
            (argument["type_"] for argument in self.arguments)
        )
        self.ir = ir.Function(program.module, self.ir_type, name=self.callee)
        self.blocks = [
            Block(program, self, i, block)
            for i, block in enumerate(data["blocks"])
        ]
        for block in self.blocks:
            block.create_instructions()
        i = 0
        while i < len(self.blocks):
            block = self.blocks[i]
            block.create_sub_blocks()
            i += 1
        i = 0
        while i < len(self.blocks):
            block = self.blocks[i]
            for j, instruction in enumerate(block.instructions):
                if isinstance(instruction, FlowInstruction):
                    block.cut(j+1, self.next_block_id())
                    break
            i += 1
        for block in self.blocks:
            block.finish()
        self.special_allocs = self.blocks[0].add_special_allocs(
            self.special_alloc_types_
        )
        self.variable_declarations = self.blocks[0].add_variable_declarations(
            self.declarations
        )
        for arg, ir_arg in zip(self.arguments, self.ir.args):
            self.blocks[0].add_argument(
                ir_arg, self.get_variable_declaration(arg["variable"]),
                arg["type_"]
            )

    def add_block(self, *args):
        block = Block(*args)
        self.blocks.append(block)
        return block

    def next_block_id(self):
        return len(self.blocks)

    def get_variable_declaration(self, id_):
        return self.variable_declarations[id_]

    def get_argument_info(self):
        for arg in self.arguments:
            yield arg["type_"], self.get_variable_declaration(arg["variable"])

    def compile_ir(self):
        for block in self.blocks:
            block.build()

    def get_special_alloc(self, i):
        return self.special_allocs[i]


class ModuleFunction:
    def __init__(self, program, data):
        self.id_ = data["id"]
        self.return_type_= data["return_type_"]
        self.arguments = data["arguments"]
        self.ir_type = make_function_type_(
            program, self.return_type_, 
            (argument["type_"] for argument in self.arguments)
        )
        self.ir = ir.Function(program.module, self.ir_type, name=data["callee"])
