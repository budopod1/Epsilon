using System;
using System.Collections.Generic;

public class Not : UnaryOperation<IValueToken>, IValueToken {
    public Not(IValueToken o) : base(o) {}

    public Type_ GetType_() {
        return new Type_("Bool");
    }
}
