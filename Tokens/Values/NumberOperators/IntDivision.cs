using System;

public class IntDivision : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public IntDivision(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return Type_.Common(o1.GetType_(), o2.GetType_());
    }
}
