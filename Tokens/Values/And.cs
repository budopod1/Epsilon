using System;

public class And : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public And(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return new Type_("Bool");
    }
}
