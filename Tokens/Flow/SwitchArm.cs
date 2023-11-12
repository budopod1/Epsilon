using System;

public class SwitchArm : BinaryOperation<ConstantValue, CodeBlock> {
    public SwitchArm(ConstantValue o1, CodeBlock o2) : base(o1, o2) {}

    public ConstantValue GetTarget() {
        return o1;
    }

    public CodeBlock GetBlock() {
        return o2;
    }
}
