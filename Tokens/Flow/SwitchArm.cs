namespace Epsilon;
public class SwitchArm(ConstantValue o1, CodeBlock o2) : BinaryAction<ConstantValue, CodeBlock>(o1, o2) {
    public ConstantValue GetTarget() {
        return o1;
    }

    public CodeBlock GetBlock() {
        return o2;
    }
}
