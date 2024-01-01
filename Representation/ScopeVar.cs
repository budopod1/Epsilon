using System;

public class ScopeVar {
    string name;
    Type_ type_;
    
    public ScopeVar(string name, Type_ type_) {
        this.name = name;
        this.type_ = type_;
    }

    public string GetName() {
        return name;
    }

    public Type_ GetType_() {
        return type_;
    }

    public IJSONValue GetJSON(int id) {
        JSONObject obj = new JSONObject();
        obj["name"] = new JSONString(name);
        obj["type_"] = type_.GetJSON();
        obj["id"] = new JSONInt(id);
        return obj;
    }
}
