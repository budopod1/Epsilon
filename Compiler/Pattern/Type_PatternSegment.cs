using System;

public class Type_PatternSegment : IPatternSegment {
    Type_ type_;
    
    public Type_PatternSegment(Type_ type_) {
        this.type_ = type_;
    }

    public bool Matches(IToken token) {
        return (token is IValueToken 
            && ((IValueToken)token).GetType_().IsConvertibleTo(type_));
    }
}
