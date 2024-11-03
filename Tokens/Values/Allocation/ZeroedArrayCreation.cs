namespace Epsilon;
public class ZeroedArrayCreation(Type_Token type_, IValueToken size) : UnaryOperation<IValueToken>(size), IValueToken, IVerifier {
    readonly Type_ type_ = type_.GetValue();

    public Type_ GetType_() {
        return type_;
    }

    public void Verify() {
        if (type_.GetBaseType_().GetName() != "Array") {
            throw new SyntaxErrorException(
                $"Non-array type {type_} cannot be zero-initialized with size", this
            );
        }
        if (!type_.GetGeneric(0).GetBaseType_().IsZeroInitializable()) {
            throw new SyntaxErrorException(
                $"Cannot create zero-initialize array of non-zero-initializable type {type_.GetGeneric(0)}", this
            );
        }
    }
}
