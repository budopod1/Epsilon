public class AddOne(IValueToken o) : UnaryOperation<IValueToken>(o), IValueToken {
    public Type_ GetType_() {
        return o.GetType_();
    }
}
