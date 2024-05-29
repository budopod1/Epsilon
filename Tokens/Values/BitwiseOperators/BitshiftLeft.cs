using System;

public class BitshiftLeft : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public BitshiftLeft(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return o1.GetType_();
    }
}
