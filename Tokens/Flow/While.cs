using System;
using System.Collections.Generic;

public class While : BinaryOperation<IValueToken, CodeBlock>, ILoop {
    public While(IValueToken o1, CodeBlock o2) : base(o1, o2) {}

    public CodeBlock GetBlock() {
        return o2;
    }

    public override int Serialize(SerializationContext context) {
        SerializationContext sub = context.AddSubContext(o2.GetScope());
        sub.Serialize(o2);
        SerializationContext conditionCtx = context.AddSubContext(hidden: true);
        o1.Serialize(conditionCtx);
        return context.AddInstruction(
            new SerializableInstruction(
                "while"
            ).AddData("block", new JSONInt(sub.GetIndex()))
             .AddData("condition", conditionCtx.Serialize())
        );
    }
}
