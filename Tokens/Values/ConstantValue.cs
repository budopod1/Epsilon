using System;

public class ConstantValue : Unit<IConstant>, IValueToken {
    public ConstantValue(IConstant constant) : base(constant) {}

    public Type_ GetType_() {
        return GetValue().GetType_();
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this).AddData("constant", GetValue().GetJSON())
        );
    }
}
