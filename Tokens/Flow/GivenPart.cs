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
        if (rawValue[0] is not IValueToken value) {
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
        Type_ optionalType_ = toType_.OptionalOf();
        Type_ valType_ = o1.GetType_();
        if (!valType_.IsCastableTo(optionalType_)) {
            throw new SyntaxErrorException(
                $"Cannot cast value of type {valType_} to type {optionalType_}", o1
            );
        }
    }
}
