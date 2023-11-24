import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path
from common import *


class BaseInstruction:
    def __init__(self, program, function, data):
        self.program = program
        self.function = function
        self.this_block = None
        self.data = data
        self.name = self.data["name"]
        self.parameters = data["parameters"]

    def finish(self, block):
        self.this_block = block

    def build(self, builder, params, param_types_):
        return self._build(builder, params, param_types_)

    def _build(self, builder, *args):
        raise NotImplementedError(f"Instruction {type(self).__name__} must define instruction()")


class Typed_Instruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.type_ = self.data["type_"]
        self.ir_type = make_type_(self.program, self.type_)


class CastToResultType_Instruction:
    def build(self, builder, params, param_types_):
        typed_params = [
            convert_type_(
                self.program, builder, param, param_type_, self.type_
            )
            for param, param_type_ in zip(params, param_types_)
        ]
        return self._build(builder, typed_params)


class ArithmeticInstruction(CastToResultType_Instruction, Typed_Instruction):
    def _build(self, builder, params):
        return {
            "addition": {0: builder.fadd, 1: builder.add, 2: builder.add},
            "subtraction": {0: builder.fsub, 1: builder.sub, 2: builder.sub},
            "multiplication": {0: builder.fmul, 1: builder.mul, 2: builder.mul},
            "division": {0: builder.fdiv, 1: builder.sdiv, 2: builder.udiv},
            "modulo": {0: builder.frem, 1: builder.srem, 2: builder.urem},
            "negation": {0: builder.fneg, 1: builder.neg, 2: builder.neg}
        }[self.name][
            (1 if is_signed_integer_type_(self.type_) else 0)
            + (2 if is_unsigned_integer_type_(self.type_) else 0)
        ](*params)


class CastInstruction(CastToResultType_Instruction, Typed_Instruction):
    def _build(self, builder, params):
        param, = params
        return param


class BitwiseInstruction(CastToResultType_Instruction, Typed_Instruction):
    def _build(self, builder, params):
        return {
            "bitwise_or": builder.or_,
            "bitwise_and": builder.and_,
            "bitwise_xor": builder.xor,
            "bitwise_not": builder.not_,
        }[self.name](*params)


class BitshiftInstruction(CastToResultType_Instruction, Typed_Instruction):
    def _build(self, builder, params):
        return {
            "bitshift_left": {True: builder.shl, False: builder.shl},
            "bitshift_right": {True: builder.lshr, False: builder.lshr},
        }[self.name][is_signed_integer_type_(self.type_)](*params)


class NotInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        param, = params
        param_type_, = param_types_
        if param_type_["name"] == "Bool":
            return builder.not_(param)
        else:
            return untruth_value(self.program, builder, param, param_type_)


class LogicalInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        return {
            "or": builder.or_,
            "and": builder.and_,
            "xor": builder.xor
        }[self.name](*[
            truth_value(self.program, builder, param, param_type_)
            for param, param_type_ in zip(params, param_types_)
        ])


class ComparisonInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.common_type_ = self.data["common_type_"]

    def _build(self, builder, params, param_types_):
        v1, v2 = [
            convert_type_(self.program, builder, param, param_type_, self.common_type_)
            for param, param_type_ in zip(params, param_types_)
        ]
        return compare_values(builder, {
            "equals": "==",
            "not_equals": "!=",
            "greater": ">",
            "less": "<",
            "greater_equal": ">=",
            "less_equal": "<="
        }[self.name], v1, v2, self.common_type_)


class AssignmentInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.variable = self.data["variable"]

    def _build(self, builder, params, param_types):
        value, = params
        value_type_, = param_types
        converted_value = convert_type_(
            self.program, builder, value, value_type_, 
            self.function.get_var(self.variable)["type_"]
        )
        return builder.store(
            converted_value, 
            self.function.get_variable_declaration(self.variable)
        )


class VariableInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.variable = self.data["variable"]

    def _build(self, builder, params, param_types):
        return builder.load(
            self.function.get_variable_declaration(self.variable)
        )


class ConstantInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.constant = make_constant(
            self.data["constant"], self.ir_type
        )

    def _build(self, builder, params, param_types):
        return self.constant


class MemberAccessInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.member = self.data["member"]
        self.struct_type_ = self.data["struct_type_"]

    def _build(self, builder, params, param_types):
        obj, = params
        struct = self.program.structs[self.struct_type_["name"]]
        return builder.load(
            builder.gep(obj, [0, struct.get_index_of_member(self.member)])
        )


class MemberAssignmentInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.member = self.data["member"]
        self.struct_type_ = self.data["struct_type_"]

    def _build(self, builder, params, param_types_):
        obj, value = params
        _, value_type_ = param_types_
        struct = self.program.structs[self.struct_type_["name"]]
        idx = struct.get_index_of_member(self.member)
        converted_value = convert_type_(
            self.program, builder, value, value_type_,
            struct.get_type__by_index(idx)
        )
        return builder.store(
            converted_value, builder.gep(obj, [0, idx])
        )


def do_chain_power(program, builder, type_, value, pow):
    mul = builder.fmul if is_floating_type_(type_) else builder.mul
    if pow == 0:
        return ir.Constant(make_type_(program, type_), 1)
    elif pow % 2 == 0:
        half = do_chain_power(
            program, builder, type_, value, pow/2
        )
        return mul(half, half)
    else:
        return mul(value, do_chain_power(
            program, builder, type_, value, pow - 1
        ))


class ExponentiationInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.mode = self.data["mode"]
        self.exponent_value = self.data["exponent_value"]

    def _build(self, builder, params, param_types_):
        base, _ = params
        base_type_, _ = param_types_
        if self.mode == "chain":
            return convert_type_(self.program, builder, do_chain_power(
                self.program, builder, base_type_, base,
                int(self.exponent_value)
            ), base_type_, self.type_)
        else:
            if self.mode == "pow":
                return self.program.call_stdlib(
                    builder, "pow", params, param_types_,
                    self.type_
                )
            elif self.mode == "sqrt":
                return self.program.call_stdlib(
                    builder, "sqrt", [base], [base_type_],
                    self.type_
                )
            elif self.mode == "cbrt":
                return self.program.call_stdlib(
                    builder, "cbrt", [base], [base_type_],
                    self.type_
                )


class DirectIRInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self._build = self.data["function"]


class FlowInstruction(BaseInstruction):
    def set_return_block(self):
        pass

    def create_sub_blocks(self):
        pass


class Condition:
    def __init__(self, program, function, data):
        self.program = program
        self.function = function
        self.block_num = data["block"]
        self.condition = data["condition"]
        self.block = self.function.blocks[self.block_num]
        self.eval_block = None
        self.next_condition = None
        self.else_block = None
        self.return_block = None

    def set_next(self, condition):
        self.next_condition = condition

    def set_else_block(self, else_block):
        self.else_block = else_block

    def set_return_block(self, return_block):
        self.return_block = return_block
        set_return_block(self.block, return_block)

    def finish(self):
        final_block = last_block_chain_block(self.eval_block)
        def _build(builder, params, param_types_):
            param, = params
            param_type_, = param_types_
            builder.cbranch(
                truth_value(self.program, builder, param, param_type_),
                self.block.block,
                (
                    (self.else_block or self.return_block) 
                    if self.next_condition is None else 
                    self.next_condition.eval_block
                ).block
            )
        final_block.instructions.append(DirectIRInstruction(
            self.program, self.function, 
            {
                "name": "direct_ir", 
                "parameters": [final_block.last_instruction_idx()], 
                "function": _build
            }
        ))

    def create_sub_block(self):
        self.eval_block = self.function.add_block(
            self.program, self.function, self.function.next_block_id(), self.condition
        )
        self.eval_block.create_instructions()


class ConditionalInstruction(FlowInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.conditions = [
            Condition(self.program, self.function, condition)
            for condition in self.data["conditions"]
        ]
        for i in range(len(self.conditions)-1):
            self.conditions[i].set_next(self.conditions[i+1])
        self.else_block = (
            self.function.blocks[self.data["else"]] 
            if self.data["else"] is not None 
            else None
        )

    def set_return_block(self):
        return_block = self.this_block.next_block
        if self.else_block is not None:
            set_return_block(self.else_block, return_block)
        for condition in self.conditions:
            condition.set_return_block(return_block)

    def create_sub_blocks(self):
        for condition in self.conditions:
            condition.create_sub_block()

    def finish(self, block):
        super().finish(block)
        if self.else_block is not None:
            self.conditions[-1].set_else_block(self.else_block)
        for condition in self.conditions:
            condition.finish()

    def _build(self, builder, params, param_types_):
        builder.branch(self.conditions[0].eval_block.block)


class WhileInstruction(FlowInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.block_num = self.data["block"]
        self.block = self.function.blocks[self.block_num]
        self.condition = self.data["condition"]
        self.eval_block = None

    def set_return_block(self):
        set_return_block(self.block, self.eval_block)

    def create_sub_blocks(self):
        self.eval_block = self.function.add_block(
            self.program, self.function, self.function.next_block_id(), self.condition
        )
        self.eval_block.create_instructions()

    def finish(self, block):
        super().finish(block)
        final_block = last_block_chain_block(self.eval_block)
        def _build(builder, params, param_types_):
            param, = params
            param_type_, = param_types_
            builder.cbranch(
                truth_value(self.program, builder, param, param_type_),
                self.block.block, self.this_block.next_block.block
            )
        final_block.instructions.append(DirectIRInstruction(
            self.program, self.function, 
            {
                "name": "direct_ir", 
                "parameters": [final_block.last_instruction_idx()], 
                "function": _build
            }
        ))

    def _build(self, builder, params, param_types_):
        builder.branch(self.eval_block.block)


class SwitchArm:
    def __init__(self, function, data, ir_type):
        self.function = function
        self.block_num = data["block"]
        self.block = self.function.blocks[self.block_num]
        self.target = make_constant(
            data["target"], ir_type
        )

    def set_return_block(self, return_block):
        set_return_block(self.block, return_block)


class SwitchInstruction(FlowInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.arms = [
            SwitchArm(self.function, arm, make_type_(
                self.program, self.data["value_type_"]
            ))
            for arm in self.data["arms"]
        ]
        self.default_block_num = self.data["default"]
        self.default_block = None

    def finish(self, block):
        super().finish(block)
        self.default_block = self.function.blocks[self.default_block_num] if self.default_block_num else None

    def _build(self, builder, params, param_types_):
        param, = params
        switch = builder.switch(
            param, (
                self.this_block.next_block 
                if self.default_block is None else 
                self.default_block
            ).block
        )
        for arm in self.arms:
            switch.add_case(arm.target, arm.block.block)

    def set_return_block(self):
        for arm in self.arms:
            arm.set_return_block(self.this_block.next_block)


class FunctionCallInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.callee = self.data["function"]

    def _build(self, builder, params, param_types_):
        func = self.program.functions[self.callee]
        converted_params = [
            convert_type_(self.program, builder, param, param_type_, argument["type_"])
            for param, param_type_, argument in zip(params, param_types_, func.arguments)
        ]
        return builder.call(func.ir, converted_params)


class ReturnVoidInstruction(BaseInstruction):
    def _build(self, builder, params, param_types_):
        return builder.ret_void()


class ReturnInstruction(BaseInstruction):
    def _build(self, builder, params, param_types_):
        param, = params
        param_type_, = param_types_
        return builder.ret(convert_type_(
            self.program, builder, param, param_type_, self.function.return_type_
        ))


class ContinueInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.block_num = self.data["block"]
        self.block = self.function.blocks[self.block_num]

    def _build(self, builder, params, param_types_):
        return builder.branch(self.block.block)


class BreakInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.block_num = self.data["block"]
        self.block = self.function.blocks[self.block_num]

    def _build(self, builder, params, param_types_):
        return builder.branch(self.block.next_block.block)


def make_instruction(program, function, data):
    return {
        "constant_value": ConstantInstruction,
        "addition": ArithmeticInstruction,
        "subtraction": ArithmeticInstruction,
        "multiplication": ArithmeticInstruction,
        "division": ArithmeticInstruction,
        "modulo": ArithmeticInstruction,
        "bitwise_not": BitwiseInstruction,
        "bitwise_and": BitwiseInstruction,
        "bitwise_xor": BitwiseInstruction,
        "bitwise_or": BitwiseInstruction,
        "bitshift_right": BitshiftInstruction,
        "bitshift_left": BitshiftInstruction,
        "not_equals": ComparisonInstruction,
        "equals": ComparisonInstruction,
        "less": ComparisonInstruction,
        "greater": ComparisonInstruction,
        "less_equal": ComparisonInstruction,
        "greater_equal": ComparisonInstruction,
        "cast": CastInstruction,
        "not": NotInstruction,
        "or": LogicalInstruction,
        "and": LogicalInstruction,
        "xor": LogicalInstruction,
        "negation": ArithmeticInstruction,
        "exponentiation": ExponentiationInstruction,
        # "array_creation": ,
        "assignment": AssignmentInstruction,
        "variable": VariableInstruction,
        # "instantiation": ,
        "member_access": MemberAccessInstruction,
        "member_assignment": MemberAssignmentInstruction,
        "return": ReturnInstruction,
        "return_void": ReturnVoidInstruction,
        "continue": ContinueInstruction,
        "break": BreakInstruction,
        "conditional": ConditionalInstruction,
        "while": WhileInstruction,
        "switch": SwitchInstruction,
        "function_call": FunctionCallInstruction,
    }[data["name"]](program, function, data)
