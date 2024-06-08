using System;

public class Return : UnaryOperation<IValueToken>, IVerifier, IFunctionTerminator, IBlockEndOnly {
    public Return(IValueToken o) : base(o) {}

    public void Verify() {
        Function func = TokenUtils.GetParentOfType<Function>(this);
        if (func.DoesReturnVoid()) {
            throw new SyntaxErrorException(
                $"Cannot return {o.GetType_()}; function expects no return value", this
            );
        }
        Type_ returnType_ = func.GetReturnType_();
        if (!o.GetType_().IsConvertibleTo(returnType_)) {
            throw new SyntaxErrorException(
                $"Cannot return {o.GetType_()}; function expects {returnType_} return type", this
            );
        }
    }

    public bool DoesTerminateFunction() {
        return true;
    }
}
