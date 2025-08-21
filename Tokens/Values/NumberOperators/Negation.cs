namespace Epsilon;
public class Negation(IValueToken o) : UnaryOperation<IValueToken>(o), IValueToken {
    public Type_ GetType_() {
        Type_ type_ = o.GetType_();
        BaseType_ bt = type_.GetBaseType_();
        if (bt.GetName() == "R") {
            return type_;
        } else {
            return new Type_("Z", bt.GetBits());
        }
    }
}
