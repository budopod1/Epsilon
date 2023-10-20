using System;

public abstract class BitwiseBinaryOp : BinaryOperation<IValueToken, IValueToken>, IValueToken, IVerifier {
    public BitwiseBinaryOp(IValueToken o1, IValueToken o2) : base(o1, o2) {}
    
    public void Verify() {
        CheckType_(o1.GetType_());
        CheckType_(o2.GetType_());
    }

    void CheckType_(Type_ type_) {
        if (!type_.IsConvertibleTo(new Type_("Z"))) {
            throw new SyntaxErrorException(
                $"Cannot perform bitwise operation on type {type_}, as it is not an integer"
            );
        }
    }

    public Type_ GetType_() {
        return Type_.Common(o1.GetType_(), o2.GetType_());
    }
}
