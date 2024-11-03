namespace Epsilon;
public class CommaOperation(ISerializableToken discard, IValueToken result) : BinaryAction<ISerializableToken, IValueToken>(discard, result), IValueToken {
    public Type_ GetType_() {
        return o2.GetType_();
    }

    public int UncachedSerialize(SerializationContext context) {
        context.Serialize(o1);
        return context.Serialize(o2);
    }
}
