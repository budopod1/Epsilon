public class While(IValueToken o1, CodeBlock o2) : BinaryAction<IValueToken, CodeBlock>(o1, o2), ILoop, IFunctionTerminator {
    public CodeBlock GetBlock() {
        return o2;
    }

    public int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["condition"] = o1,
            ["block"] = o2
        }.Register();
    }

    public bool DoesTerminateFunction() {
        if (o1 is not ConstantValue constantT) return false;
        if (!constantT.GetValue().IsTruthy()) return false;
        TraverseConfig config = new(
            TraverseMode.DEPTH, invert: false, yieldFirst: false,
            avoidTokens: token => token is ILoop
        );
        foreach (Break break_ in TokenUtils.TraverseFind<Break>(o2, config)) {
            return false;
        }
        return true;
    }
}
