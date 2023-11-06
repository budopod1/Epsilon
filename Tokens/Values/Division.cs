using System;

public class Division : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public Division(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return Type_.CommonSpecific(
            o1.GetType_(), o2.GetType_(), "Q"
        );
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
        );
    }
}
