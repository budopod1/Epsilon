public class BinaryBoolOperator(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), IValueToken {
    public Type_ GetType_() {
        return new Type_("Bool");
    }

    public override int UncachedSerialize(SerializationContext ctx) {
        return new SerializableInstruction(ctx, this) {
            ["o2"] = o2
        }.SetOperands([o1]).Register();
    }
}
