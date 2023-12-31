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
        self.scope = data["scope"]
        self.is_main = data["is_main"]
        self.ir_type = make_function_type_(
            program, self.return_type_, 
            (argument["type_"] for argument in self.arguments)
        )
        self.ir = ir.Function(
            program.module, self.ir_type,
            name=("main" if self.is_main else "func"+str(id_))
        )
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
        self.variable_declarations = self.blocks[0].add_variable_declarations(
            self.scope
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
    
    def get_var(self, id_):
        return self.scope[str(id_)]

    def compile_ir(self):
        for block in self.blocks:
            block.build()
