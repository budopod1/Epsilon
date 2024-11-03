namespace Epsilon;
public class Abort(IValueToken o) : UnaryOperation<IValueToken>(o), IFunctionTerminator, IBlockEndOnly {
    public bool DoesTerminateFunction() {
        return true;
    }
}
