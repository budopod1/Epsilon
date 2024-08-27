public class GivenPart : BinaryAction<IValueToken, CodeBlock>, IVerifier {
    readonly Type_ toType_;
    readonly int varID;

    public GivenPart(RawGivenPart part) : base(null, part.GetBlock()) {
        span = part.span;
        RawGivenValue rawValue = part.GetRawValue();
        if (rawValue.Count != 1) {
            throw new SyntaxErrorException(
                "Expected a single value", rawValue
            );
        }
        IValueToken value = rawValue[0] as IValueToken;
        if (value == null) {
            throw new SyntaxErrorException(
                "Expected a value", rawValue
            );
        }
        o1 = value;
        toType_ = part.GetToType_();
        varID = part.GetVarID();
    }

    public IValueToken GetValue() {
        return o1;
    }

    public Type_ GetToType_() {
        return toType_;
    }

    public int GetVarID() {
        return varID;
    }

    public CodeBlock GetBlock() {
        return o2;
    }

    public void Verify() {
        if (toType_.GetBaseType_().GetName() == "Optional") {
            throw new SyntaxErrorException(
                $"Given part will always be triggered as specified type is {toType_}", this
            );
        }
        Type_ fromType_ = toType_.OptionalOf();
        Type_ valType_ = o1.GetType_();
        if (!valType_.IsCastableTo(fromType_)) {
            throw new SyntaxErrorException(
                $"Cannot cast value of type {valType_} to type {fromType_}", o1
            );
        }
    }
}
