using System;
using System.Collections.Generic;

public class While : BinaryOperation<IValueToken, CodeBlock>, ILoop, IFunctionTerminator {
    public While(IValueToken o1, CodeBlock o2) : base(o1, o2) {}

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
        ConstantValue constantT = o1 as ConstantValue;
        if (constantT == null) return false;
        if (!constantT.GetValue().IsTruthy()) return false;
        TraverseConfig config = new TraverseConfig(
            TraverseMode.DEPTH, invert: false, yieldFirst: false, 
            avoidTokens: token => token is ILoop
        );
        foreach (Break break_ in TokenUtils.TraverseFind<Break>(o2, config)) {
            return false;
        }
        return true;
    }
}
