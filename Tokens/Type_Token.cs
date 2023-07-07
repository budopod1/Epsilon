using System;

public class Type_Token : IToken {
    Type_ type_;
    
    public Type_Token(Type_ type_) {
        this.type_ = type_;
    }

    public Type_ GetType_() {
        return type_;
    }

    public override string ToString() {
        return Utils.WrapName("Type_Token", type_.ToString());
    }
}
