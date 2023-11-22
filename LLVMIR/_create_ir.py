import llvmlite.binding as llvm
from llvmlite import ir
import orjson
from pathlib import Path


REF_COUNTER_FIELD = ir.IntType(32)


def traverse(json):
    yield json
    if isinstance(json, list):
        for elem in json:
            yield from traverse(elem)
    elif isinstance(json, dict):
        for elem in json.values():
            yield from traverse(elem)


def freeze_json(json):
    if isinstance(json, list):
        return tuple([
            freeze_json(elem) for elem in json
        ])
    elif isinstance(json, dict):
        return frozenset({
            (key, freeze_json(value))
            for key, value in json.items()
        })
    return json


def make_type_(program, data):
    generics = [
        make_type_(program, generic)
        for generic in data["generics"]
    ]

    bits = data["bits"]
    match data["name"], generics:
        case "Q", []:
            return {
                16: ir.HalfType,
                32: ir.FloatType,
                64: ir.DoubleType
            }[bits]()
        case ("W" | "Z" | "Bool" | "Byte"), []:
            return ir.IntType(bits)
        case "Void", []:
            return ir.VoidType()
        case "Pointer", [pointee]:
            return pointee.as_pointer()
        case "Array", [sub]:
            id_ = program.array_ids[freeze_json(data)]
            for id_ in program.arrays:
                return program.arrays[id_].ir_type
            else:
                return ir.global_context.get_identified_type(
                    "___a"+str(id_)
                ).as_pointer()
        case name, []:
            if name in program.structs:
                return program.structs[name].ir_type
            else:
                return ir.global_context.get_identified_type(
                    name
                ).as_pointer()

    assert False, f"Invalid type {data}"


def make_constant(constant, ir_type):
    return ir.Constant(
        ir_type,
        constant["value"]
    )


def is_integer_type_(type_):
    return type_["name"] in ["W", "Z", "Bool", "Byte"]


def is_floating_type_(type_):
    return type_["name"] in ["Q"]


def is_signed_integer_type_(type_):
    return type_["name"] in ["Z"]


def is_unsigned_integer_type_(type_):
    return type_["name"] in ["W", "Bool", "Byte"]


def convert_floating_type__bits(builder, val, old, new, new_type):
    oldb = old["bits"]
    newb = new["bits"]
    if newb > oldb:
        return builder.fpext(val, new_type)
    elif newb < oldb:
        return builder.fptrunc(val, new_type)
    else:
        return val


def convert_integer_type__bits(builder, val, old, new, new_type):
    oldb = old["bits"]
    newb = new["bits"]
    if newb > oldb:
        if is_signed_integer_type_(old):
            return builder.sext(val, new_type)
        else:
            return builder.zext(val, new_type)
    elif newb < oldb:
        return builder.trunc(val, new_type)
    else:
        return val


def convert_type_(program, builder, val, old, new):
    if old == new:
        return val
    new_ir_type = make_type_(program, new)
    if is_integer_type_(old) and is_integer_type_(new):
        return convert_integer_type__bits(builder, val, old, new, new_ir_type)
    elif is_floating_type_(old) and is_floating_type_(new):
        return convert_floating_type__bits(builder, val, old, new, new_ir_type)
    elif is_floating_type_(old) and is_signed_integer_type_(new):
        return builder.fptosi(val, new_ir_type)
    elif is_floating_type_(old) and is_unsigned_integer_type_(new):
        return builder.fptoui(val, new_ir_type)
    elif is_signed_integer_type_(old) and is_floating_type_(new):
        return builder.sitofp(val, new_ir_type)
    elif is_unsigned_integer_type_(old) and is_floating_type_(new):
        return builder.uitofp(val, new_ir_type)
    raise TypeError(f"Cannot convert type {old} to {new}")


class Array:
    def __init__(self, program, id_, type_):
        self.program = program
        self.id_ = id_
        self.type_ = type_
        self.generic = type_["generics"][0]
        field_ir_types = [
            REF_COUNTER_FIELD, 
            ir.IntType(32), 
            ir.PointerType(make_type_(program, self.generic))
        ]
        self.ir_type = ir.LiteralStructType(field_ir_types).as_pointer()
        ir.global_context.get_identified_type(
            "___a"+str(id_)
        ).set_body(*field_ir_types)


class Struct:
    def __init__(self, program, name, fields):
        self.program = program
        self.name = name
        self.fields = fields
        field_ir_types = [REF_COUNTER_FIELD] + [
            make_type_(program, field["type_"])
            for field in fields
        ]
        self.ir_type = ir.LiteralStructType(field_ir_types).as_pointer()
        ir.global_context.get_identified_type(
            name
        ).set_body(*field_ir_types)

    def get_index_of_member(self, member):
        for i, field in enumerate(self.fields):
            if field["name"] == member:
                return i
        raise KeyError(f"Struct {name} has no member {member}")

    def get_type__by_index(self, index):
        return self.fields[index]["type_"]


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


def compare_values(builder, comparison, value1, value2, type_):
    if is_floating_type_(type_):
        return builder.fcmp_unordered(comparison, value1, value2)
    elif is_signed_integer_type_(type_):
        return builder.icmp_signed(comparison, value1, value2)
    else:
        return builder.icmp_unsigned(comparison, value1, value2)


def truth_value(program, builder, value, type_):
    if is_integer_type_(type_) and type_["bits"] == 1:
        return value
    if is_floating_type_(type_):
        return builder.fcmp_unordered("!=", value, ir.Constant(make_type_(program, type_), 0))
    elif is_signed_integer_type_(type_):
        return builder.icmp_signed("!=", value, ir.Constant(make_type_(program, type_), 0))
    else:
        return builder.icmp_unsigned("!=", value, ir.Constant(make_type_(program, type_), 0))


def untruth_value(program, builder, value, type_):
    if is_integer_type_(type_) and type_["bits"] == 1:
        return builder.not_(value)
    if is_floating_type_(type_):
        return builder.fcmp_unordered("==", value, ir.Constant(make_type_(program, type_), 0))
    elif is_signed_integer_type_(type_):
        return builder.icmp_signed("==", value, ir.Constant(make_type_(program, type_), 0))
    else:
        return builder.icmp_unsigned("==", value, ir.Constant(make_type_(program, type_), 0))


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


class DirectIRInstruction(BaseInstruction):
    def __init__(self, *args):
        super().__init__(*args)
        self._build = self.data["function"]


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
        self.eval_block = Block(
            self.program, self.function, self.function.next_block_id(), self.condition
        )
        self.function.blocks.append(self.eval_block)
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
        self.eval_block = Block(
            self.program, self.function, self.function.next_block_id(), self.condition
        )
        self.function.blocks.append(self.eval_block)
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
        # "exponentiation": ,
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
        next_block = Block(
            self.program, self.function, id_, cut,
            start + self.param_offset, True
        )
        self.next_block = next_block
        return next_block


class Function:
    def __init__(self, program, id_, data):
        self.program = program
        self.id_ = id_
        self.return_type_= data["return_type_"]
        self.arguments = data["arguments"]
        self.scope = data["scope"]
        self.ir_type = ir.FunctionType(
            make_type_(program, self.return_type_),
            [
                make_type_(program, argument["type_"]) 
                for argument in self.arguments
            ]
        )
        self.ir = ir.Function(program.module, self.ir_type,
                              name="___f"+str(id_))
        self.blocks = [
            Block(program, self, i, block)
            for i, block in enumerate(data["instructions"])
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
                    self.blocks.append(block.cut(j+1, self.next_block_id()))
                    break
            i += 1
        for block in self.blocks:
            block.finish()
        self.variable_declarations = self.blocks[0].add_variable_declarations(
            self.scope
        )
        for arg, ir_arg in zip(self.arguments, self.ir.args):
            self.blocks[0].add_argument(
                ir_arg, self.get_variable_declaration(arg["variable"])
            )

    def next_block_id(self):
        return len(self.blocks)

    def get_variable_declaration(self, id_):
        return self.variable_declarations[id_]

    def get_var(self, id_):
        return self.scope[str(id_)]

    def compile_ir(self):
        for block in self.blocks:
            block.build()


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


def create_ir(data):
    module = ir.Module(name="main")
    
    module.triple = llvm.get_default_triple()
    
    program = Program(module)

    program.array_ids = dict(map(
        lambda pair: (freeze_json(pair[1]), pair[0]), 
        enumerate(data["arrays"])
    ))
    
    for i, array in enumerate(data["arrays"]):
        program.add_array(Array(
            program, i, array
        ))

    for struct in data["structs"]:
        program.add_struct(Struct(
            program, struct["name"], struct["fields"]
        ))

    functions = []
    for function_data in data["functions"]:
        function = Function(
            program, function_data["id"], function_data
        )
        program.add_function(function)
        functions.append(function)

    for function in program.functions.values():
        function.compile_ir()

    return module


def main(*, print_ir=False):
    with open("code.json") as file:
        data = orjson.loads(file.read())

    module = create_ir(data)

    if print_ir:
        print(module)

    with open("code.ll", "w") as file:
        file.write(str(module))


if __name__ == "__main__":
    main(print_ir=True)
