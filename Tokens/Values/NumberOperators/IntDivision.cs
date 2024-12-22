namespace Epsilon;
public class IntDivision(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), IValueToken, ICanAbort {
    public bool CanAbort() {
        if (o2 is not ConstantValue constant) return true;
        if (constant.GetValue() is not IIntConstant intConstant) return true;
        return intConstant.GetIntValue() == 0;
    }

    public Type_ GetType_() {
        return Type_.CommonNonNull(this, o1.GetType_(), o2.GetType_());
    }
}
