using System;

public class GreaterEqual : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public GreaterEqual(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return new Type_("Bool");
    }
}
