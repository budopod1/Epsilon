public class StringLiteral(string str) : IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly string str = str;

    public Type_ GetType_() {
        return Type_.String();
    }

    public virtual int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["string"] = str
        }.Register();
    }
}
