using System;
using System.Collections.Generic;

public class Negation : UnaryOperation<IValueToken>, IValueToken {
    public Negation(IValueToken o) : base(o) {}

    public Type_ GetType_() {
        Type_ type_ = o.GetType_();
        BaseType_ bt = type_.GetBaseType_();
        if (bt.GetName() == "Q") {
            return type_;
        } else {
            return new Type_("Z", bt.GetBits());
        }
    }
}
