public class Group(IValueToken o) : UnaryOperation<IValueToken>(o), IValueToken {
    public Type_ GetType_() {
        return o.GetType_();
    }

    public override int UncachedSerialize(SerializationContext context) {
        return context.Serialize(o);
    }
}
