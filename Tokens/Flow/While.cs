using System;

public class While : BinaryOperation<IValueToken, CodeBlock>, IFlowControl {
    public While(IValueToken o1, CodeBlock o2) : base(o1, o2) {}
}