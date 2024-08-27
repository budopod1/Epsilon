public class NullValue : IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public Type_ GetType_() {
        return new Type_("Null");
    }

    public int Serialize(SerializationContext context) {
        return new SerializableInstruction(context, this).Register();
    }
}
