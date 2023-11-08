using System;
using System.Collections.Generic;

public class While : BinaryOperation<IValueToken, CodeBlock>, IFlowControl {
    public While(IValueToken o1, CodeBlock o2) : base(o1, o2) {}

    public override int Serialize(SerializationContext context) {
        SerializationContext sub = context.AddSubContext();
        sub.Serialize(o2);
        return context.AddInstruction(
            new SerializableInstruction(
                "while", new List<int> {o1.Serialize(context)}
            ).AddData("block", new JSONInt(sub.GetIndex()))
        );
    }
}
