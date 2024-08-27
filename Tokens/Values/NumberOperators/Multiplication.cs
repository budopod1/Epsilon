public class Multiplication(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), IValueToken {
    public Type_ GetType_() {
        return Type_.CommonNonNull(this, o1.GetType_(), o2.GetType_());
    }
}
