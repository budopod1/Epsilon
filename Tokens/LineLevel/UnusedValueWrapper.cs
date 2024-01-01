using System;

public class UnusedValueWrapper : UnaryOperation<IValueToken>, ICompleteLine {
    public UnusedValueWrapper(IValueToken o) : base(o) {}
}
