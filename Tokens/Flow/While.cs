using System;
using System.Collections.Generic;

public class While(IValueToken o1, CodeBlock o2) : BinaryOperation<IValueToken, CodeBlock>(o1, o2), ILoop, IFunctionTerminator {
    public CodeBlock GetBlock() {
        return o2;
    }

    public override int Serialize(SerializationContext context) {
        SerializationContext sub = context.AddSubContext();
        sub.Serialize(o2);
        SerializationContext conditionCtx = context.AddSubContext(hidden: true);
        conditionCtx.SerializeInstruction(o1);
        return context.AddInstruction(
            new SerializableInstruction(this)
                .AddData("block", new JSONInt(sub.GetIndex()))
                .AddData("condition", conditionCtx.Serialize())
        );
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
