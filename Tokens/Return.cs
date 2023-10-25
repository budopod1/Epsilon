using System;

public class Return : UnaryOperation<IValueToken>, IVerifier, ICompleteLine {
    public Return(IValueToken o) : base(o) {}

    public void Verify() {
        Function func = TokenUtils.GetParentOfType<Function>(this);
        if (!o.GetType_().IsConvertibleTo(func.GetReturnType_())) {
            throw new SyntaxErrorException(
                "Invalid return type"
            );
        }
    }
}
