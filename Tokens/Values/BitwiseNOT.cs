using System;

public class BitwiseNOT : UnaryOperation<IValueToken>, IValueToken {
    public BitwiseNOT(IValueToken o) : base(o) {}

    public Type_ GetType_() {
        return o.GetType_();
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
        );
    }
}
