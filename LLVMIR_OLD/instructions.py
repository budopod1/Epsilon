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

    def __str__(self):
        return f"{type(self).__name__}"


class Typed_Instruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.type_ = self.data["type_"]
        self.ir_type = make_type_(self.program, self.type_)


class CastToResultType_Instruction(Typed_Instruction):
    def build(self, builder, params, param_types_):
        typed_params = [
            convert_type_(
                self.program, builder, param, param_type_, self.type_
            )
            for param, param_type_ in zip(params, param_types_)
        ]
        return self._build(builder, typed_params)


class FlowInstruction(BaseInstruction):
    def set_return_block(self):
        pass

    def create_sub_blocks(self):
        pass

    def add_sub_branch_instructions(self):
        pass


class AbortInstruction(BaseInstruction):
    def _build(self, builder, params, param_types_):
        string, = params
        self.program.call_extern(builder, "abort_", [string], [String], None)


class AbortVoidInstruction(BaseInstruction):
    def _build(self, builder, params, param_types_):
        self.program.call_extern(builder, "exit", [i32_of(1)], [Z32], None)


class AddOneInstruction(CastToResultType_Instruction):
    def _build(self, builder, params):
        value, = params
        return (
            builder.fadd if is_floating_type_(self.type_) else builder.add
        )(value, make_type_(self.program, self.type_)(1))


class ArithmeticInstruction(CastToResultType_Instruction):
    def _build(self, builder, params):
        return {
            "addition": {0: builder.fadd, 1: builder.add, 2: builder.add},
            "subtraction": {0: builder.fsub, 1: builder.sub, 2: builder.sub},
            "multiplication": {0: builder.fmul, 1: builder.mul, 2: builder.mul},
            "division": {0: builder.fdiv, 1: builder.sdiv, 2: builder.udiv},
            "int_division": {0: builder.fdiv, 1: builder.sdiv, 2: builder.udiv},
            "modulo": {0: builder.frem, 1: builder.srem, 2: builder.urem},
            "negation": {0: builder.fneg, 1: builder.neg, 2: builder.neg}
        }[self.name][
            (1 if is_signed_integer_type_(self.type_) else 0)
            + (2 if is_unsigned_integer_type_(self.type_) else 0)
        ](*params)


class ArrayAccessInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        array, index = params
        array_type_, index_type_ = param_types_
        # array should always already be the correct type_
        # this could change, and if it does, type_ casting would be
        # required here
        casted_index = convert_type_(
            self.program, builder, index, index_type_,
            W64
        )
        self.program.verify_array_idx(builder, array, casted_index)
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
        # array should always already be the correct type_
        # this could change, and if it does, type_ casting would be
        # required here
        casted_index = convert_type_(
            self.program, builder, index, index_type_,
            W64
        )
        if not is_value_type_(value_type_):
            incr_ref_counter(self.program, builder, casted_value, value_type_)
        casted_value = convert_type_(
            self.program, builder, value, value_type_,
            self.elem_type_
        )
        result_ptr = builder.load(builder.gep(
            array, [i64_of(0), i32_of(3)]
        ))
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
                builder, "incrementArrayRefCounts", 
                [struct_mem, self.program.make_elem(builder, self.elem_type_)],
                [self.type_, W64], None
            )
        return struct_mem


class AssignmentInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.variable = self.data["variable"]
        self.var_type_ = self.data["var_type_"]

    def _build(self, builder, params, param_types_):
        value, = params
        value_type_, = param_types_
        var_type_ = self.var_type_
        if not is_value_type_(value_type_):
            incr_ref_counter(self.program, builder, value, value_type_)
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
        return builder.store(converted_value, declaration)


class BitshiftInstruction(CastToResultType_Instruction):
    def _build(self, builder, params):
        return {
            "bitshift_left": {True: builder.shl, False: builder.shl},
            "bitshift_right": {True: builder.lshr, False: builder.lshr},
        }[self.name][is_signed_integer_type_(self.type_)](*params)


class BitwiseInstruction(CastToResultType_Instruction):
    def _build(self, builder, params):
        return {
            "bitwise_or": builder.or_,
            "bitwise_and": builder.and_,
            "bitwise_xor": builder.xor,
            "bitwise_not": builder.not_,
        }[self.name](*params)


class BranchInstruction(FlowInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.true_block = self.data["true_block"]
        self.false_block = self.data["false_block"]

    def set_branch_this_block(self, this_block):
        self.this_block = this_block

    def _build(self, builder, params, param_types_):
        param, = params
        param_type_, = param_types_
        cond = truth_value(self.program, builder, param, param_type_)
        self.program.check_ref(builder, param, param_type_)
        self.this_block.perpare_for_termination()
        builder.cbranch(cond, self.true_block.block, self.false_block.block)


class BreakInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.block_num = self.data["block"]
        self.block = self.function.blocks[self.block_num]

    def _build(self, builder, params, param_types_):
        self.this_block.perpare_for_termination()
        return builder.branch(self.block.break_block.block)


class CastInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        param, = params
        param_type_, = param_types_
        return convert_type_(self.program, builder, param, param_type_, self.type_)


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

    def add_sub_branch_instructions(self):
        final_block = last_block_chain_block(self.eval_block)
        final_block.instructions.append(BranchInstruction(
            self.program, self.function, 
            {
                "name": "branch", 
                "parameters": [final_block.last_instruction_idx()], 
                "true_block": self.block,
                "false_block": (
                    (self.else_block or self.return_block) 
                    if self.next_condition is None else 
                    self.next_condition.eval_block
                ),
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
        self.conditions[-1].set_else_block(self.else_block)

    def set_return_block(self):
        return_block = self.this_block.next_block
        if self.else_block is not None:
            set_return_block(self.else_block, return_block)
        for condition in self.conditions:
            condition.set_return_block(return_block)

    def create_sub_blocks(self):
        for condition in self.conditions:
            condition.create_sub_block()

    def add_sub_branch_instructions(self):
        for condition in self.conditions:
            condition.add_sub_branch_instructions()

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
        return builder.branch(self.block.continue_block.block)


class EqInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.common_type_ = self.data["common_type_"]

    def _build(self, builder, params, param_types_):
        type_ = self.common_type_
        v1, v2 = [
            convert_type_(self.program, builder, param, param_type_, type_)
            for param, param_type_ in zip(params, param_types_)
        ]
        return self.program.refrence_equals(
            builder, type_, v1, v2, self.name=="not_equals"
        )


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


class ForInstruction(FlowInstruction):
    FOR_COUNTER = 0
    
    def __init__(self, *args):
        super().__init__(*args)
        self.block_num = self.data["block"]
        self.block = self.function.blocks[self.block_num]
        self.variable = self.data["variable"]
        self.clause_names = self.data["clause_names"]
        # self.check_block_num = self.data["check_block"]
        self.type_ = self.data["type_"]
        self.for_counter = self.FOR_COUNTER
        self.FOR_COUNTER += 1
        self.check_block = None
        self.finish_block = None
        self.iter_var = self.data["iter_var"]
        self.iter_type_ = self.data["iter_type_"]

    def finish(self, block):
        super().finish(block)
        self.check_block = self.function.ir.append_basic_block(
            f"for_check{self.for_counter}"
        )
        self.finish_block = self.function.ir.append_basic_block(
            f"for_finish{self.for_counter}"
        )

    def set_return_block(self):
        check_block_wrapped = IRBlockWrapper(self.check_block)
        set_return_block(self.block, check_block_wrapped)
        self.block.break_block = self.this_block.next_block
        self.block.continue_block = check_block_wrapped

    def _build(self, builder, params, param_types_):
        
        iter_var = self.function.get_special_alloc(self.iter_var)

        loop_block = self.block.block
        exit_block = self.this_block.next_block.block
        
        is_in = "in" in self.clause_names
        is_enumerating = "enumerating" in self.clause_names
        
        idx_type_ = self.iter_type_
        ir_idx_type = make_type_(self.program, idx_type_)
        
        from_ = ir_idx_type(0)
        to = None
        array = None
        step = ir_idx_type(1)
        step_type_ = idx_type_
        signed_step = False
        
        zipped = zip(params, param_types_, self.clause_names)
        for param, param_type_, clause_name in zipped:
            if clause_name == "from":
                from_ = convert_type_(
                    self.program, builder, param, param_type_, idx_type_
                )
            elif clause_name == "to":
                to = convert_type_(
                    self.program, builder, param, param_type_, idx_type_
                )
            elif clause_name == "in" or clause_name == "enumerating":
                array = param
                to = convert_type_(
                    self.program, builder,
                    builder.load(builder.gep(param, [i64_of(0), i32_of(2)])),
                    W64, idx_type_
                )
            elif clause_name == "step":
                signed_step = is_signed_integer_type_(param_type_)
                step = param
                
        start = from_
        end = to
        
        negative_step = None
        if signed_step:
            zero = make_type_(self.program, step_type_)(0)
            negative_step = builder.icmp_signed("<", step, zero)
            
            real_to = builder.sub(to, ir_idx_type_(1))
            start = builder.select(negative_step, real_to, from_)
            end = builder.select(negative_step, from_, to)

        idx_comparer = (
            builder.icmp_signed if is_signed_integer_type_(idx_type_)
            else builder.icmp_unsigned
        )

        start_lt_end = idx_comparer("<", start, end)
        start_loop = start_lt_end
        
        if signed_step:
            end_lt_start = idx_comparer("<", end, start)
            start_loop = builder.select(
                negative_step, end_lt_start, start_lt_end
            )

        declaration = self.function.get_variable_declaration(self.variable)

        builder.store(start, iter_var)
        
        if is_in:
            with builder.if_then(start_loop):
                value = builder.load(builder.gep(builder.load(
                    builder.gep(array, [i64_of(0), i32_of(3)])
                ), [start]))
                if not is_value_type_(self.type_):
                    incr_ref_counter(self.program, builder, value, self.type_)
                builder.store(value, declaration)
                builder.branch(loop_block)
            builder.branch(self.finish_block)
        else:
            builder.store(start, declaration)
            builder.cbranch(start_loop, loop_block, self.finish_block)

        builder = ir.IRBuilder(self.check_block)

        idx_comparer = (
            builder.icmp_signed if is_signed_integer_type_(idx_type_)
            else builder.icmp_unsigned
        )

        idx = builder.add(builder.load(iter_var), step)

        idx_lt_end = idx_comparer("<", idx, end)
        continue_ = idx_lt_end

        if signed_step:
            idx_ge_start = idx_comparer(">=", idx, start)
            continue_ = builder.select(negative_step, idx_ge_start, idx_lt_end)

        builder.store(idx, iter_var)

        if is_in:
            with builder.if_then(continue_):
                value = builder.load(builder.gep(builder.load(
                    builder.gep(array, [i64_of(0), i32_of(3)])
                ), [idx]))
                if not is_value_type_(self.type_):
                    incr_ref_counter(self.program, builder, value, self.type_)
                builder.store(value, declaration)
                builder.branch(loop_block)
            builder.branch(self.finish_block)
        else:
            builder.store(idx, declaration)
            builder.cbranch(continue_, loop_block, self.finish_block)

        builder = ir.IRBuilder(self.finish_block)
        
        for param, param_type_ in zip(params, param_types_):
            self.program.check_ref(builder, param, param_type_)
            
        builder.branch(exit_block)


class FormatChainInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        template, *params = params
        _, *param_types_ = param_types_
        
        arr_type_ = ir.ArrayType(make_type_(self.program, String), len(params))
        value_array = builder.alloca(arr_type_)
        
        strs_to_check_ref = [template]
        strs_to_free = []
        
        for i, (param, param_type_) in enumerate(zip(params, param_types_)):
            if param_type_ == String:
                strs_to_check_ref.append(param)
                str_ = param
            else:
                str_ = self.program.stringify(builder, param, param_type_)
                self.program.check_ref(builder, param, param_type_)
                strs_to_free.append(str_)
            builder.store(str_, builder.gep(value_array, [i64_of(0), i64_of(i)]))

        result = builder.call(self.program.extern_funcs["formatString"], [
            template, builder.gep(value_array, [i64_of(0), i64_of(0)]), i32_of(len(params))
        ])

        for str in strs_to_check_ref:
            self.program.check_ref(builder, str, String)

        for str in strs_to_free:
            arr_content = builder.load(builder.gep(str, [i64_of(0), i32_of(3)]))
            self.program.dumb_free(builder, arr_content)
            self.program.dumb_free(builder, str)

        return result


class FunctionCallInstruction(Typed_Instruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.callee = self.data["function"]

    def _build(self, builder, params, param_types_):
        if self.program.is_builtin(self.callee):
            return self.program.call_builtin(
                self.callee, builder, params, param_types_, self.type_
            )
        else:
            func = self.program.get_function(self.callee)
            return self.program.call_function(
                builder, func, params, param_types_
            )

class InitialAssignmentInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.variable = self.data["variable"]
        self.var_type_ = self.data["var_type_"]

    def _build(self, builder, params, param_types_):
        value, = params
        value_type_, = param_types_
        var_type_ = self.var_type_
        if not is_value_type_(value_type_):
            incr_ref_counter(self.program, builder, value, value_type_)
        converted_value = convert_type_(
            self.program, builder, value, value_type_, 
            var_type_
        )
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
            if not is_value_type_(param_type_):
                incr_ref_counter(self.program, builder, param, param_type_)
            proper_type_ = struct.get_type__by_index(idx)
            converted = convert_type_(
                self.program, builder, param, param_type_,
                proper_type_
            )
            casted_fields.append(converted)
        init_ref_counter(builder, result)
        for idx, casted_field in enumerate(casted_fields):
            builder.store(casted_field, builder.gep(result, [i64_of(0), i32_of(1+idx)]))
        return result


class IntDivisionInstruction(CastToResultType_Instruction):
    def _build(self, builder, params):
        a, b = params
        # whether or not b is signed for the comparison doesn't matter because the
        # llvmlite functions icmp_unsigned and icmp_signed when using cmpop "=="
        # both generate the same `icmp eq` ir
        div_0 = builder.icmp_unsigned("==", b, ir.Constant(b.type, 0))
        div_0 = self.program.expect(builder, div_0, False)
        with builder.if_then(div_0):
            self.program.call_extern(builder, "div0Fail", [], [], None)
            builder.unreachable()
        if is_signed_integer_type_(self.type_):
            return builder.sdiv(a, b)
        else:
            return builder.udiv(a, b)


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
        struct = self.program.structs[self.struct_type_["name"]]
        idx = struct.get_index_of_member(self.member)
        result_type_ = struct.get_type__by_index(idx)
        if not is_value_type_(value_type_):
            incr_ref_counter(self.program, builder, value, value_type_)
        converted_value = convert_type_(
            self.program, builder, value, value_type_, result_type_
        )
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


class NullInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        return self.program.nullptr(
            builder, self.ir_type
        )


class OptionalArrayAccessInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        array, index = params
        array_type_, index_type_ = param_types_
        casted_index = convert_type_(
            self.program, builder, index, index_type_, W64
        )
        return self.program.optional_array_access(
            builder, array, array_type_, self.type_, casted_index
        )


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
        capacity = str_len+1
        array_mem = self.program.string_literal_array(
            builder, self.string, capacity, unique=True
        )
        struct_mem = self.program.malloc(
            builder, make_type_(self.program, String).pointee
        )
        init_ref_counter(builder, struct_mem)
        builder.store(
            i64_of(capacity),
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
        return struct_mem



class SubOneInstruction(CastToResultType_Instruction):
    def _build(self, builder, params):
        value, = params
        return (
            builder.fsub if is_floating_type_(self.type_) else builder.sub
        )(value, make_type_(self.program, self.type_)(1))


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


class UnusedValueInstruction(BaseInstruction):
    def _build(self, builder, params, param_types_):
        param, = params
        param_type_, = param_types_
        self.program.check_ref(builder, param, param_type_)


class VoidFunctionCallInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self.callee = self.data["function"]
    
    def _build(self, builder, params, param_types_):
        if self.program.is_builtin(self.callee):
            self.program.call_builtin(
                self.callee, builder, params, param_types_, None
            )
        else:
            func = self.program.get_function(self.callee)
            self.program.call_function(
                builder, func, params, param_types_
            )


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
        self.block.break_block = self.this_block.next_block
        self.block.continue_block = self.block

    def create_sub_blocks(self):
        self.eval_block = self.function.add_block(
            self.program, self.function, self.function.next_block_id(), self.condition
        )
        self.eval_block.create_instructions()

    def add_sub_branch_instructions(self):
        final_block = last_block_chain_block(self.eval_block)
        final_block.instructions.append(BranchInstruction(
            self.program, self.function, 
            {
                "name": "branch", 
                "parameters": [final_block.last_instruction_idx()], 
                "true_block": self.block,
                "false_block": self.this_block.next_block
            }
        ))

    def _build(self, builder, params, param_types_):
        self.this_block.perpare_for_termination()
        builder.branch(self.eval_block.block)


class ZeroedArrayCreationInstruction(Typed_Instruction):
    def _build(self, builder, params, param_types_):
        size, = params
        size_type_, = param_types_
        ir_generic_type_ = make_type_(self.program, self.type_["generics"][0])
        elem_size = self.program.sizeof(builder, ir_generic_type_)
        return self.program.call_extern(
            builder, "makeBlankArray", [size, elem_size],
            [size_type_, W64], self.type_
        )


def make_instruction(program, function, data):
    return {
        "abort": AbortInstruction,
        "abort_void": AbortVoidInstruction,
        "add_one": AddOneInstruction,
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
        # "branch": BranchInstruction,
        "constant_value": ConstantInstruction,
        "continue": ContinueInstruction,
        "division": ArithmeticInstruction,
        "equals": EqInstruction,
        "exponentiation": ExponentiationInstruction,
        "for": ForInstruction,
        "format_chain": FormatChainInstruction,
        "function_call": FunctionCallInstruction,
        "greater": ComparisonInstruction,
        "greater_equal": ComparisonInstruction,
        "initial_assignment": InitialAssignmentInstruction,
        "instantiation": InstantiationInstruction,
        "int_division": IntDivisionInstruction,
        "less": ComparisonInstruction,
        "less_equal": ComparisonInstruction,
        "member_access": MemberAccessInstruction,
        "member_assignment": MemberAssignmentInstruction,
        "modulo": ArithmeticInstruction,
        "multiplication": ArithmeticInstruction,
        "negation": ArithmeticInstruction,
        "not": NotInstruction,
        "not_equals": EqInstruction,
        "null_value": NullInstruction,
        "optional_array_access": OptionalArrayAccessInstruction,
        "or": LogicalInstruction,
        "return": ReturnInstruction,
        "return_void": ReturnVoidInstruction,
        "string_literal": StringLiteralInstruction,
        "sub_one": SubOneInstruction,
        "subtraction": ArithmeticInstruction,
        "switch": SwitchInstruction,
        "uninit_var_declaration": NoopInstruction,
        "unused_value_wrapper": UnusedValueInstruction,
        "variable": VariableInstruction,
        "void_function_call": VoidFunctionCallInstruction,
        "while": WhileInstruction,
        "xor": LogicalInstruction,
        "zeroed_array_creation": ZeroedArrayCreationInstruction,
    }[data["name"]](program, function, data)
