using System;
using System.Collections.Generic;

public class Group(IValueToken o) : UnaryOperation<IValueToken>(o), IValueToken {
    public Type_ GetType_() {
        return o.GetType_();
    }

    public override int Serialize(SerializationContext context) {
        return context.SerializeInstruction(o);
    }
}
