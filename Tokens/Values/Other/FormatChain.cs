public class FormatChain : IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public IValueToken template;
    public List<IValueToken> values;

    public int Count {
        get {
            return values.Count + 1;
        }
    }

    public IToken this[int i] {
        get {
            if (i == 0) return template;
            return values[i - 1];
        }
        set {
            if (i == 0) {
                template = (IValueToken)value;
            } else {
                values[i - 1] = (IValueToken)value;
            }
        }
    }

    public FormatChain(FormatChain chain, IValueToken nextValue) {
        template = chain.GetTemplate();
        values = chain.GetValues();
        values.Add(nextValue);
    }

    public FormatChain(IValueToken template, IValueToken value1) {
        this.template = template;
        values = [value1];
    }

    public IValueToken GetTemplate() {
        return template;
    }

    public List<IValueToken> GetValues() {
        return values;
    }

    public Type_ GetType_() {
        return Type_.String();
    }

    public override string ToString() {
        return Utils.WrapName(
            GetType().Name,
            template.ToString() + ": " + string.Join(
                ", ", values.ConvertAll<string>(
                    obj => obj.ToString()
                )
            )
        );
    }

    public int Serialize(SerializationContext context) {
        return new SerializableInstruction(context, this).Register();
    }
}
