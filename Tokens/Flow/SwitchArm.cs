using System;

public class SwitchArm : BinaryOperation<IValueToken, CodeBlock> {
    public SwitchArm(IValueToken o1, CodeBlock o2) : base(o1, o2) {}

    public IValueToken GetTarget() {
        return o1;
    }

    public CodeBlock GetBlock() {
        return o2;
    }
}
