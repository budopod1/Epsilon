using System;

public class Abort : UnaryOperation<IValueToken>, IFunctionTerminator, IBlockEndOnly {
    public Abort(IValueToken o) : base(o) {}

    public bool DoesTerminateFunction() {
        return true;
    }
}
