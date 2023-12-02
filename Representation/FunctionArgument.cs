using System;
using System.Collections.Generic;

public class FunctionArgument {
    string name;
    Type_ type_;
    int id;
    
    public FunctionArgument(string name, Type_ type_, int id = -1) {
        this.name = name;
        this.type_ = type_;
        this.id = id;
    }

    public FunctionArgument(FunctionArgumentToken token) {
        this.name = token.GetName();
        this.type_ = token.GetType_();
        this.id = token.GetID();
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
