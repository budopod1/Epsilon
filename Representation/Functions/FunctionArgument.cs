using System;
using System.Collections.Generic;

public class FunctionArgument : IEquatable<FunctionArgument> {
    string name;
    Type_ type_;
    bool exactType_Match;
    int id;

    public FunctionArgument(string name, Type_ type_, bool exactType_Match=false, int id = -1) {
        this.name = name;
        this.type_ = type_;
        this.exactType_Match = exactType_Match;
        this.id = id;
    }

    public FunctionArgument(FunctionArgumentToken token) {
        this.name = token.GetName();
        this.type_ = token.GetType_();
        exactType_Match = false;
        this.id = token.GetID();
    }

    public string GetName() {
        return name;
    }

    public bool IsCompatibleWith(Type_ other) {
        if (exactType_Match) {
            return other.Matches(type_);
        } else {
            return other.IsConvertibleTo(type_);
        }
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

    public bool Equals(FunctionArgument other) {
        if (name != other.GetName()) return false;
        if (id != other.GetID()) return false;
        return type_.Equals(other.GetType_());
    }
}
