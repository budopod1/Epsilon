using System;

public class If : BinaryOperation<IValueToken, CodeBlock> {
    public If(IValueToken condition, CodeBlock block) : base(condition, block) {}

    public IValueToken GetCondition() {
        return o1;
    }

    public CodeBlock GetIfBlock() {
        return o2;
    }
}
