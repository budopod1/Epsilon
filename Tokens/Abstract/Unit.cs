public abstract class Unit<T>(T value) : IToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly T value = value;

    public T GetValue() {
        return value;
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, value.ToString());
    }
}
