namespace Epsilon;
public class Modulo(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), ICanAbort, IValueToken {
    public bool CanAbort() {
        return GetType_().GetBaseType_().IsInt() && (
            o2 is not ConstantValue constant
            || constant.GetValue() is not IIntConstant intConstant
            || intConstant.GetIntValue() == 0
        );
    }

    public Type_ GetType_() {
        return Type_.CommonNonNull(this, o1.GetType_(), o2.GetType_());
    }
}
