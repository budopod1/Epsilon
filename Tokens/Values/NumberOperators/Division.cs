namespace Epsilon;
public class Division(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), IValueToken {
    public Type_ GetType_() {
        return Type_.CommonSpecificNonNull(
            this, o1.GetType_(), o2.GetType_(), "R"
        );
    }
}
