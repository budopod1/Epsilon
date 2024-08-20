using System;

public class Or(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), IValueToken {
    public Type_ GetType_() {
        return new Type_("Bool");
    }
}
