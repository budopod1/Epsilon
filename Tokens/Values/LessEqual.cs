using System;

public class LessEqual : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public LessEqual(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return new Type_("Bool");
    }
}
