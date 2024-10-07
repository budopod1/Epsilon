public class Instantiation : IParentToken, IValueToken, IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly Type_ type_;
    readonly List<IValueToken> values;

    public int Count {
        get {
            return values.Count;
        }
    }

    public IToken this[int i] {
        get {
            return values[i];
        }
        set {
            values[i] = (IValueToken)value;
        }
    }

    public Instantiation(Type_ type_, List<IValueToken> values) {
        this.type_ = type_;
        this.values = values;
    }

    public Instantiation(Type_Token type_token, ValueList list) {
        type_ = type_token.GetValue();
        values = [];
        foreach (ValueListItem listItem in list.GetValues()) {
            if (listItem.Count == 0) continue;
            if (listItem.Count != 1 || !(listItem[0] is IValueToken)) {
                throw new SyntaxErrorException("Malformed instantiation parameter", listItem);
            }
            values.Add((IValueToken)listItem[0]);
        }
    }

    public Type_ GetType_() {
        return type_;
    }

    public List<IValueToken> GetValues() {
        return values;
    }

    public override string ToString() {
        return Utils.WrapName(
            Utils.WrapName(
                GetType().Name,
                type_.ToString(),
                "<", ">"
            ),
            string.Join(
                ", ", values.ConvertAll<string>(
                    obj => obj.ToString()
                )
            )
        );
    }

    public void Verify() {
        Struct struct_ = StructsCtx.GetStructFromType_(type_);
        List<Field> fields = struct_.GetFields().ToList();
        if (fields.Count != values.Count) {
            throw new SyntaxErrorException(
                $"{values.Count} values were supplied to an instantiation of struct {type_}, while {fields.Count} values are required.", this
            );
        }
        for (int i = 0; i < fields.Count; i++) {
            IValueToken value = values[i];
            Type_ valueType_ = value.GetType_();
            Type_ fieldType_ = fields[i].GetType_();
            if (!valueType_.IsConvertibleTo(fieldType_)) {
                throw new SyntaxErrorException(
                    $"Expected value of type {fieldType_} in instantiation, got value of type {valueType_}", value
                );
            }
        }
    }

    public int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this).Register();
    }
}
