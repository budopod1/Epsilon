import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path
from common import *
from instructions import make_instruction, BaseInstruction, FlowInstruction


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
        self.registered_values = []
        self.consumed_values = set()

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
            name=("b"+str(self.id_) if self.id_ > 0 else "entry")
        )
        self.builder = ir.IRBuilder(self.block)

    def build(self):
        ir_instructions = []
        instruction_result_types_ = []
        for instruction in self.instructions:
            assert not self.builder.block.is_terminated,\
                "Cannot add instruction to terminated block"
            result_type_ = getattr(instruction, "type_", None)
            instruction_result_types_.append(result_type_)
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
            if result_type_ is not None and built is not None and instruction.REGISTER_RESULT:
                self.register_value(built, result_type_)
            ir_instructions.append(built)

        if not self.builder.block.is_terminated:
            if self.return_block:
                self.perpare_for_termination()
                self.builder.branch(self.return_block.block)
            else:
                self.prepare_for_return()
                self.builder.ret_void()

    def add_variable_declarations(self, scope):
        variable_declarations = {}
        for str_id, scopevar in scope.items():
            variable_declarations[int(str_id)] = self.builder.alloca(
                make_type_(self.program, scopevar["type_"])
            )
        return variable_declarations

    def add_argument(self, value, var, type_):
        self.builder.store(value, var)
        if not is_value_type_(type_):
            incr_ref_counter(self.builder, value)

    def perpare_for_termination(self):
        for type_, value in self.registered_values:
            if value in self.consumed_values:
                continue
            self.program.check_ref(self.builder, value, type_)

    def prepare_for_return(self, ret_val=None, ret_type_=None):
        if ret_val is not None and not is_value_type_(ret_type_):
            incr_ref_counter(self.builder, ret_val)
        self.perpare_for_termination()
        for type_, var in self.function.get_argument_info():
            if not is_value_type_(type_):
                self.program.decr_ref(
                    self.builder, self.builder.load(var), type_
                )
        if ret_val is not None and not is_value_type_(ret_type_):
            dumb_decr_ref_counter(self.builder, ret_val)

    def register_value(self, value, type_):
        if is_value_type_(type_):
            return
        self.registered_values.append((type_, value))

    def consume_value(self, value):
        self.consumed_values.add(value)

    def cut(self, start, id_):
        cut = self.instructions[start:]
        self.instructions = self.instructions[:start]
        next_block = self.function.add_block(
            self.program, self.function, id_, cut,
            start + self.param_offset, True
        )
        self.next_block = next_block
        return next_block
