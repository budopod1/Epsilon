using System;

public class Subtraction : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public Subtraction(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return Type_.Common(o1.GetType_(), o2.GetType_());
    }
}
