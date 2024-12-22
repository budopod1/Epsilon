namespace Epsilon;
public class Abort(IValueToken o) : UnaryOperation<IValueToken>(o), IFunctionTerminator, IBlockEndOnly, ICanAbort {
    public bool CanAbort() {
        return true;
    }

    public bool DoesTerminateFunction() {
        return true;
    }
}
