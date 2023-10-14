using System;

public class Condition : BinaryOperation<IValueToken, CodeBlock> {
    public Condition(IValueToken o1, CodeBlock o2) : base(o1, o2) {}

    public IValueToken GetCondition() {
        return o1;
    }

    public CodeBlock GetBlock() {
        return o2;
    }
}
