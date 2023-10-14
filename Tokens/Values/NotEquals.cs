using System;

public class NotEquals : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public NotEquals(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return new Type_("Bool");
    }
}
