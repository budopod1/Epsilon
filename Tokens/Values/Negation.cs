using System;
using System.Collections.Generic;

public class Negation : UnaryOperation<IValueToken>, IValueToken {
    public Negation(IValueToken o) : base(o) {}

    public Type_ GetType_() {
        return o.GetType_();
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
        );
    }
}
