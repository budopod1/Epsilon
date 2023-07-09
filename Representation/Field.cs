using System;

public class Field {
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
}
