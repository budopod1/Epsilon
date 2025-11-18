namespace Epsilon;
public class Cast : UnaryOperation<IValueToken>, IValueToken, IVerifier {
    readonly Type_ type_;
#if NEW_CAST
    public Cast(IValueToken o, Unit<Type_> utype_) : base(o) {
        type_ = utype_.GetValue();
    }
#else
    CodeSpan type_Span;

    public Cast(Unit<Type_> utype_, IValueToken o) : base(o) {
        type_Span = new CodeSpan(utype_.span.GetStart() + 1, utype_.span.GetEnd() - 1);
        type_ = utype_.GetValue();
    }
#endif

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
#if !NEW_CAST
        SyntaxUpdater.Substitute(this, [o, "#", type_Span]);
#endif
        if (!o.GetType_().IsCastableTo(type_)) {
            throw new SyntaxErrorException(
                $"Cannot cast value of type {o.GetType_()} to {type_}", this
            );
        }
    }
}
