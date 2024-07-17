using System;

public class Field : IEquatable<Field> {
    string name;
    Type_ type_;

    public Field(string name, Type_ type_) {
        this.name = name;
        this.type_ = type_;
    }

    public Field(VarDeclaration declaration) {
        this.name = declaration.GetName().GetValue();
        this.type_ = declaration.GetType_();
    }

    public string GetName() {
        return name;
    }

    public Type_ GetType_() {
        return type_;
    }

    public override string ToString() {
        return $"{type_}:{name}";
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["name"] = new JSONString(name);
        obj["type_"] = type_.GetJSON();
        return obj;
    }

    public bool Equals(Field other) {
        return name == other.GetName() && type_.Equals(other.GetType_());
    }
}
