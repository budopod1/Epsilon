public abstract class UnaryOperation<T>(T o) : UnaryAction<T>(o), ISerializableToken where T : IValueToken {
    public virtual int Serialize(SerializationContext context) {
        return new SerializableInstruction(context, this).SetOperands([o]).Register();
    }
}
