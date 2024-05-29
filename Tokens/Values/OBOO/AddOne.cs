using System;
using System.Collections.Generic;

public class AddOne : UnaryOperation<IValueToken>, IValueToken {
    public AddOne(IValueToken o) : base(o) {}

    public Type_ GetType_() {
        return o.GetType_();
    }
}
