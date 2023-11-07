using System;
using System.Collections.Generic;

public class FunctionArgumentToken : IToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    string name;
    Type_ type_;
    int id;
    
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

    public int GetID() {
        return id;
    }

    public void SetID(int id) {
        this.id = id;
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, type_.ToString() + ":" + name);
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["name"] = new JSONString(name);
        obj["type_"] = type_.GetJSON();
        obj["variable"] = new JSONInt(id);
        return obj;
    }
}
