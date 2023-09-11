using System;

public class Assignment : BinaryOperation<IValueToken, IValueToken> {
    public Assignment(Variable o1, IValueToken o2) : base(o1, o2) {}
}
