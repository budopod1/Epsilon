using System;
using System.Collections.Generic;

public class SubOne : UnaryOperation<IValueToken>, IValueToken {
    public SubOne(IValueToken o) : base(o) {}

    public Type_ GetType_() {
        return o.GetType_();
    }
}
