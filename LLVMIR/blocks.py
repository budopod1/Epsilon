import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path
from common import *
from instructions import make_instruction, BaseInstruction, FlowInstruction


def iter_block_chain(block_chain):
    block = block_chain
    while block is not None:
        yield block
        block = block.next_block


def last_block_chain_block(block_chain):
    result = None
    for block in iter_block_chain(block_chain):
        result = block
    return result


def set_return_block(block_chain, return_block):
    for block in iter_block_chain(block_chain):
        block.return_block = return_block


class Block:
    def __init__(self, program, function, id_, instructions, param_offset=0, real_instructions=False):
        self.id_ = id_
        self.program = program
        self.raw_instructions = None if real_instructions else instructions
        self.instructions = instructions if real_instructions else None
        self.function = function
        self.block = None
        self.builder = None
        self.next_block = None
        self.return_block = None
        self.param_offset = param_offset

    def create_instructions(self):
        self.instructions = [
            instruction
            if isinstance(instruction, BaseInstruction) else
            make_instruction(self.program, self.function, instruction)
            for instruction in self.raw_instructions
        ]

    def create_sub_blocks(self):
        for instruction in self.instructions:
            if isinstance(instruction, FlowInstruction):
                instruction.create_sub_blocks()

    def set_return_blocks(self):
        for instruction in self.instructions:
            if isinstance(instruction, FlowInstruction):
                instruction.set_return_block()

    def last_instruction_idx(self):
        return self.param_offset + len(self.instructions)-1

    def finish(self):
        for instruction in self.instructions:
            instruction.finish(self)
        self.set_return_blocks()
        self.block = self.function.ir.append_basic_block(
            name=("___b"+str(self.id_) if self.id_ > 0 else "entry")
        )
        self.builder = ir.IRBuilder(self.block)

    def build(self):
        ir_instructions = []
        instruction_result_types_ = []
        for instruction in self.instructions:
            assert not self.builder.block.is_terminated,\
                "Cannot add instruction to terminated block"
            instruction_result_types_.append(
                getattr(instruction, "type_", None)
            )
            params, param_types_ = [], []
            if instruction.parameters:
                params, param_types_ = zip(*[
                    (
                        ir_instructions[parameter-self.param_offset], 
                        instruction_result_types_[parameter-self.param_offset]
                    )
                    for parameter in instruction.parameters
                ])
            built = instruction.build(self.builder, params, param_types_)
            ir_instructions.append(built)

        if not self.builder.block.is_terminated:
            if self.return_block:
                self.builder.branch(self.return_block.block)
            else:
                self.builder.ret_void()

    def add_variable_declarations(self, scope):
        variable_declarations = {}
        for str_id, scopevar in scope.items():
            variable_declarations[int(str_id)] = self.builder.alloca(
                make_type_(self.program, scopevar["type_"])
            )
        return variable_declarations

    def add_argument(self, value, var):
        self.builder.store(value, var)

    def cut(self, start, id_):
        cut = self.instructions[start:]
        self.instructions = self.instructions[:start]
        next_block = self.function.add_block(
            self.program, self.function, id_, cut,
            start + self.param_offset, True
        )
        self.next_block = next_block
        return next_block
