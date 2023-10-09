using System;

public class While : BinaryOperation<IValueToken, CodeBlock> {
    public While(IValueToken o1, CodeBlock o2) : base(o1, o2) {}
}
