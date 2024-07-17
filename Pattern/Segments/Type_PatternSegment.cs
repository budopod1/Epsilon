using System;

public class Type_PatternSegment : IPatternSegment {
    Type_ type_;

    public Type_ GetType_() {
        return type_;
    }

    public Type_PatternSegment(Type_ type_) {
        this.type_ = type_;
    }

    public bool Matches(IToken token) {
        return (token is IValueToken
            && ((IValueToken)token).GetType_().IsConvertibleTo(type_));
    }

    public bool Equals(IPatternSegment obj) {
        Type_PatternSegment other = obj as Type_PatternSegment;
        if (other == null) return false;
        return type_ == other.GetType_();
    }
}
