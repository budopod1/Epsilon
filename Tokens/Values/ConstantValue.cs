using System;

public class ConstantValue : Unit<int>, IValueToken {
    public ConstantValue(int constant) : base(constant) {}

    public Type_ GetType_() {
        Program program = TokenUtils.GetParentOfType<Program>(this);
        Constants constants = program.GetConstants();
        IConstant constant = constants.GetConstant(GetValue());
        return constant.GetType_();
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction("constant_value", null, GetType_())
                .AddData("id", new JSONInt(GetValue()))
        );
    }
}
