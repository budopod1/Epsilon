using System;

public class ConstantValue(IConstant constant) : Unit<IConstant>(constant), IValueToken {
    public Type_ GetType_() {
        return GetValue().GetType_();
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this).AddData("constant", GetValue().GetJSON())
        );
    }
}
