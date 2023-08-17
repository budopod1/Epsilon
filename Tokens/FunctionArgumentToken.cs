using System;
using System.Collections.Generic;

public class FunctionArgumentToken : IToken {
    public IParentToken parent { get; set; }
    
    string name;
    Type_ type_;
    
    public FunctionArgumentToken(string name, Type_ type_) {
        this.name = name;
        this.type_ = type_;
    }

    public string GetName() {
        return name;
    }

    public Type_ GetType_() {
        return type_;
    }

    public override string ToString() {
        return Utils.WrapName(this.GetType().Name, type_.ToString() + ":" + name);
    }
}
