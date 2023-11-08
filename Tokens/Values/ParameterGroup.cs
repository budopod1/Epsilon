using System;
using System.Collections.Generic;

public class ParameterGroup : UnaryOperation<IValueToken>, IValueToken {
    public ParameterGroup(IValueToken o) : base(o) {}
    
    public Type_ GetType_() {
        return o.GetType_();
    }

    public override int Serialize(SerializationContext context) {
        return o.Serialize(context);
    }
}
