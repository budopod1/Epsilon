public abstract class BinaryOperation<T1, T2>(T1 o1, T2 o2) : BinaryAction<T1, T2>(o1, o2), ISerializableToken where T1 : IValueToken where T2 : IValueToken {
    public virtual int Serialize(SerializationContext ctx) {
        return new SerializableInstruction(ctx, this).SetOperands([o1, o2]).Register();
    }
}
