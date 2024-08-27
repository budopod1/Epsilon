public class Field : IEquatable<Field> {
    readonly string name;
    readonly Type_ type_;

    public Field(string name, Type_ type_) {
        this.name = name;
        this.type_ = type_;
    }

    public Field(VarDeclaration declaration) {
        name = declaration.GetName().GetValue();
        type_ = declaration.GetType_();
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
        return new JSONObject {
            ["name"] = new JSONString(name),
            ["type_"] = type_.GetJSON()
        };
    }

    public bool Equals(Field other) {
        return name == other.GetName() && type_.Equals(other.GetType_());
    }
}
