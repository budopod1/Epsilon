using System;
using System.Collections.Generic;

public class Group : UnaryOperation<IValueToken>, IValueToken {
    public Group(IValueToken o) : base(o) {}

    public Type_ GetType_() {
        return o.GetType_();
    }
}
