from llvmlite import ir
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


class DirectIRInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self._build = self.data["function"]


class FlowInstruction(BaseInstruction):
    def set_return_block(self):
        pass

    def create_sub_blocks(self):
        pass


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


class ArrayAccessInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        array, index = params
        _, index_type_ = param_types_
        # array should always already be the correct type_
        # this could change, and if it does, type_ casting would be
        # required here
        casted_index = convert_type_(
            self.program, builder, index, index_type_,
            W32
        )
        result_ptr = builder.load(builder.gep(
            array, [i64_of(0), i32_of(3)]
        ))
        return builder.load(builder.gep(
            result_ptr, [casted_index]
        ))


class ArrayAssignmentInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.elem_type_ = self.data["elem_type_"]

    def _build(self, builder, params, param_types_):
        array, index, value = params
        _, index_type_, value_type_ = param_types_
        self.this_block.consume_value(value)
        # array should always already be the correct type_
        # this could change, and if it does, type_ casting would be
        # required here
        casted_index = convert_type_(
            self.program, builder, index, index_type_,
            W64
        )
        casted_value = convert_type_(
            self.program, builder, value, value_type_,
            self.elem_type_
        )
        result_ptr = builder.load(builder.gep(
            array, [i64_of(0), i32_of(3)]
        ))
        if not is_value_type_(self.elem_type_):
            incr_ref_counter(builder, casted_index)
        builder.store(casted_value, builder.gep(
            result_ptr, [casted_index]
        ))


class ArrayCreationInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.elem_type_ = self.data["elem_type_"]

    def _build(self, builder, elems, elem_types_):
        converted_elems = [
            convert_type_(
                self.program, builder, elem, elem_type_,
                self.elem_type_
            )
            for elem, elem_type_ in zip(elems, elem_types_)
        ]
        struct_mem = self.program.malloc(
            builder, make_type_(self.program, self.type_).pointee
        )
        init_ref_counter(builder, struct_mem)
        capacity = max(10, len(elems))
        builder.store(
            i64_of(capacity),
            builder.gep(struct_mem, [i64_of(0), i32_of(1)]),
        )
        builder.store(
            i64_of(len(elems)),
            builder.gep(struct_mem, [i64_of(0), i32_of(2)]),
        )
        array_mem = self.program.malloc(
            builder, make_type_(
                self.program, self.elem_type_
            ), capacity
        )
        builder.store(
            array_mem,
            builder.gep(struct_mem, [i64_of(0), i32_of(3)])
        )
        for i, elem in enumerate(converted_elems):
            builder.store(elem, builder.gep(array_mem, [i64_of(i)]))
        if not is_value_type_(self.elem_type_):
            self.program.call_extern(
                builder, "alwaysIncrementArrayRefCounts", 
                [struct_mem, self.program.sizeof(
                    builder, make_type_(self.program, self.elem_type_)
                )],
                [self.type_, W64],
                VOID
            )
        self.this_block.register_value(struct_mem, self.type_)
        return struct_mem


class AssignmentInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.variable = self.data["variable"]

    def _build(self, builder, params, param_types):
        value, = params
        value_type_, = param_types
        self.this_block.consume_value(value)
        var_type_ = self.function.get_var(self.variable)["type_"]
        converted_value = convert_type_(
            self.program, builder, value, value_type_, 
            var_type_
        )
        declaration = self.function.get_variable_declaration(
            self.variable
        )
        self.program.decr_ref(
            builder, builder.load(declaration), var_type_
        )
        if not is_value_type_(var_type_):
            incr_ref_counter(builder, converted_value)
        return builder.store(converted_value, declaration)


class BitshiftInstruction(CastToResultType_Instruction, Typed_Instruction):
    def _build(self, builder, params):
        return {
            "bitshift_left": {True: builder.shl, False: builder.shl},
            "bitshift_right": {True: builder.lshr, False: builder.lshr},
        }[self.name][is_signed_integer_type_(self.type_)](*params)


class BitwiseInstruction(CastToResultType_Instruction, Typed_Instruction):
    def _build(self, builder, params):
        return {
            "bitwise_or": builder.or_,
            "bitwise_and": builder.and_,
            "bitwise_xor": builder.xor,
            "bitwise_not": builder.not_,
        }[self.name](*params)


class BreakInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.block_num = self.data["block"]
        self.block = self.function.blocks[self.block_num]

    def _build(self, builder, params, param_types_):
        self.this_block.perpare_for_termination()
        return builder.branch(self.block.next_block.block)


class CastInstruction(CastToResultType_Instruction, Typed_Instruction):
    def _build(self, builder, params):
        param, = params
        return param


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
            final_block.perpare_for_termination()
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
        self.this_block.perpare_for_termination()
        builder.branch(self.conditions[0].eval_block.block)


class ConstantInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.constant = make_constant(
            self.data["constant"], self.ir_type
        )

    def _build(self, builder, params, param_types):
        return self.constant


class ContinueInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.block_num = self.data["block"]
        self.block = self.function.blocks[self.block_num]

    def _build(self, builder, params, param_types_):
        self.this_block.perpare_for_termination()
        return builder.branch(self.block.block)


class ExponentiationInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.mode = self.data["mode"]
        self.exponent_value = self.data["exponent_value"]

    def _build(self, builder, params, param_types_):
        base, _ = params
        base_type_, _ = param_types_
        if self.mode == "chain":
            ev = round(self.exponent_value)
            if ev == 0:
                return make_type_(self.program, self.type_)(1)
            elif ev > 0:
                return convert_type_(self.program, builder, do_chain_power(
                    self.program, builder, base_type_, base, ev
                ), base_type_, self.type_)
            elif ev < 0:
                return builder.fdiv(
                    make_type_(self.program, self.type_)(1),
                    convert_type_(self.program, builder, do_chain_power(
                        self.program, builder, base_type_, base, abs(ev)
                    ), base_type_, self.type_)
                )
        else:
            if self.mode == "pow":
                return self.program.call_extern(
                    builder, "pow", params, param_types_,
                    self.type_
                )
            elif self.mode == "sqrt":
                return self.program.call_extern(
                    builder, "sqrt", [base], [base_type_],
                    self.type_
                )
            elif self.mode == "cbrt":
                return self.program.call_extern(
                    builder, "cbrt", [base], [base_type_],
                    self.type_
                )


class FunctionCallInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.callee = self.data["function"]

    def _build(self, builder, params, param_types_):
        for param in params:
            self.this_block.consume_value(param)
        if self.program.is_builtin(self.callee):
            return self.program.call_builtin(
                self.callee, builder, params, param_types_, self.type_
            )
        else:
            func = self.program.functions[self.callee]
            converted_params = [
                convert_type_(self.program, builder, param, param_type_, argument["type_"])
                for param, param_type_, argument in zip(params, param_types_, func.arguments)
            ]
            return builder.call(func.ir, converted_params)


class InitialAssignment(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.variable = self.data["variable"]

    def _build(self, builder, params, param_types_):
        value, = params
        value_type_, = param_types_
        self.this_block.consume_value(value)
        var_type_ = self.function.get_var(self.variable)["type_"]
        converted_value = convert_type_(
            self.program, builder, value, value_type_, 
            var_type_
        )
        if not is_value_type_(var_type_):
            incr_ref_counter(builder, converted_value)
        return builder.store(
            converted_value, 
            self.function.get_variable_declaration(self.variable)
        )


class InstantiationInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        result = self.program.malloc(builder, make_type_(self.program, self.type_).pointee)
        struct = self.program.structs[self.type_["name"]]
        casted_fields = []
        for idx, (param, param_type_) in enumerate(zip(params, param_types_)):
            proper_type_ = struct.get_type__by_index(idx)
            converted = convert_type_(
                self.program, builder, param, param_type_,
                proper_type_
            )
            if not is_value_type_(proper_type_):
                incr_ref_counter(builder, converted)
            casted_fields.append(converted)
        init_ref_counter(builder, result)
        for idx, casted_field in enumerate(casted_fields):
            builder.store(casted_field, builder.gep(result, [i64_of(0), i32_of(1+idx)]))
        self.this_block.register_value(result, self.type_)
        return result


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


class MemberAccessInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.member = self.data["member"]
        self.struct_type_ = self.data["struct_type_"]

    def _build(self, builder, params, param_types):
        obj, = params
        struct = self.program.structs[self.struct_type_["name"]]
        return builder.load(
            builder.gep(obj, [
                i64_of(0), i32_of(1+struct.get_index_of_member(self.member))
            ])
        )


class MemberAssignmentInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.member = self.data["member"]
        self.struct_type_ = self.data["struct_type_"]

    def _build(self, builder, params, param_types_):
        obj, value = params
        _, value_type_ = param_types_
        self.this_block.consume_value(value)
        struct = self.program.structs[self.struct_type_["name"]]
        idx = struct.get_index_of_member(self.member)
        result_type_ = struct.get_type__by_index(idx)
        converted_value = convert_type_(
            self.program, builder, value, value_type_, result_type_
        )
        if not is_value_type_(result_type_):
            incr_ref_counter(builder, converted_value)
        return builder.store(
            converted_value, builder.gep(obj, [i64_of(0), i32_of(1+idx)])
        )


class NoopInstruction(BaseInstruction):
    def _build(self, *arg):
        pass


class NotInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        param, = params
        param_type_, = param_types_
        if param_type_["name"] == "Bool":
            return builder.not_(param)
        else:
            return untruth_value(self.program, builder, param, param_type_)


class ReturnInstruction(BaseInstruction):
    def _build(self, builder, params, param_types_):
        param, = params
        param_type_, = param_types_
        ret_type_ = self.function.return_type_
        ret_val = convert_type_(
            self.program, builder, param, param_type_, ret_type_
        )
        self.this_block.prepare_for_return(ret_val, ret_type_)
        return builder.ret(ret_val)


class ReturnVoidInstruction(BaseInstruction):
    def _build(self, builder, params, param_types_):
        self.this_block.prepare_for_return()
        return builder.ret_void()


class StringLiteralInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.string = self.data["string"]

    def _build(self, builder, _1, _2):
        str_len = len(self.string)
        array_mem = self.program.string_literal_array(
            builder, self.string, str_len, unique=True
        )
        struct_mem = self.program.malloc(
            builder, make_type_(self.program, String).pointee
        )
        init_ref_counter(builder, struct_mem)
        builder.store(
            i64_of(str_len),
            builder.gep(struct_mem, [i64_of(0), i32_of(1)]),
        )
        builder.store(
            i64_of(str_len),
            builder.gep(struct_mem, [i64_of(0), i32_of(2)]),
        )
        builder.store(
            array_mem,
            builder.gep(struct_mem, [i64_of(0), i32_of(3)])
        )
        self.this_block.register_value(struct_mem, self.type_)
        return struct_mem


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
        self.this_block.perpare_for_termination()
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


class VariableInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.variable = self.data["variable"]

    def _build(self, builder, params, param_types):
        return builder.load(
            self.function.get_variable_declaration(self.variable)
        )


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
            final_block.perpare_for_termination()
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
        self.this_block.perpare_for_termination()
        builder.branch(self.eval_block.block)


def make_instruction(program, function, data):
    return {
        "addition": ArithmeticInstruction,
        "and": LogicalInstruction,
        "array_access": ArrayAccessInstruction,
        "array_assignment": ArrayAssignmentInstruction,
        "array_creation": ArrayCreationInstruction,
        "assignment": AssignmentInstruction,
        "bitshift_left": BitshiftInstruction,
        "bitshift_right": BitshiftInstruction,
        "bitwise_and": BitwiseInstruction,
        "bitwise_not": BitwiseInstruction,
        "bitwise_or": BitwiseInstruction,
        "bitwise_xor": BitwiseInstruction,
        "break": BreakInstruction,
        "cast": CastInstruction,
        "conditional": ConditionalInstruction,
        "constant_value": ConstantInstruction,
        "continue": ContinueInstruction,
        "division": ArithmeticInstruction,
        "equals": ComparisonInstruction,
        "exponentiation": ExponentiationInstruction,
        "function_call": FunctionCallInstruction,
        "greater": ComparisonInstruction,
        "greater_equal": ComparisonInstruction,
        "initial_assignment": InitialAssignment,
        "instantiation": InstantiationInstruction,
        "less": ComparisonInstruction,
        "less_equal": ComparisonInstruction,
        "member_access": MemberAccessInstruction,
        "member_assignment": MemberAssignmentInstruction,
        "modulo": ArithmeticInstruction,
        "multiplication": ArithmeticInstruction,
        "negation": ArithmeticInstruction,
        "not": NotInstruction,
        "not_equals": ComparisonInstruction,
        "or": LogicalInstruction,
        "return": ReturnInstruction,
        "return_void": ReturnVoidInstruction,
        "string_literal": StringLiteralInstruction,
        "subtraction": ArithmeticInstruction,
        "switch": SwitchInstruction,
        "uninit_var_declaration": NoopInstruction,
        "variable": VariableInstruction,
        "while": WhileInstruction,
        "xor": LogicalInstruction,
    }[data["name"]](program, function, data)
