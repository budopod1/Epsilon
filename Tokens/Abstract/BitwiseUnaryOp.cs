using System;

public abstract class BitwiseUnaryOp : UnaryOperation<IValueToken>, IValueToken, IVerifier {
    public BitwiseUnaryOp(IValueToken o) : base(o) {}

    public void Verify() {
        Type_ type_ = o.GetType_();
        if (!type_.IsConvertibleTo(new Type_("Z"))) {
            throw new SyntaxErrorException(
                $"Cannot perform bitwise operation on type {type_}, as it is not an integer"
            );
        }
    }

    public Type_ GetType_() {
        return o.GetType_();
    }
}
