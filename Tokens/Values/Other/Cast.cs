namespace Epsilon;
public class Cast : UnaryOperation<IValueToken>, IValueToken, IVerifier {
    readonly Type_ type_;

    public Cast(Type_ type_, IValueToken o) : base(o) {
        this.type_ = type_;
    }

    public Cast(Unit<Type_> utype_, IValueToken o) : base(o) {
        type_ = utype_.GetValue();
    }

    public Type_ GetType_() {
        return type_;
    }

    public override string ToString() {
        return Utils.WrapName(
            Utils.WrapName(
                GetType().Name,
                type_.ToString(),
                "<", ">"
            ),
            o.ToString()
        );
    }

    public void Verify() {
        if (!o.GetType_().IsCastableTo(type_)) {
            throw new SyntaxErrorException(
                $"Cannot cast value of type {o.GetType_()} to {type_}", this
            );
        }
    }
}
