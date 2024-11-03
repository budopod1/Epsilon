namespace Epsilon;
public class Condition(IValueToken o1, CodeBlock o2) : BinaryAction<IValueToken, CodeBlock>(o1, o2) {
    public IValueToken GetCondition() {
        return o1;
    }

    public CodeBlock GetBlock() {
        return o2;
    }
}
