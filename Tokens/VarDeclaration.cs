using System;

public class VarDeclaration : IParentToken {
    public IParentToken parent { get; set; }
    
    Type_ type_;
    Name name;
    
    public int Count {
        get { return 1; }
    }
    
    public IToken this[int i] {
        get {
            return name;
        }
        set {
            name = (Name)value;
        }
    }
    
    public VarDeclaration(Type_ type_, Name name) {
        this.type_ = type_;
        this.name = name;
    }
    
    public VarDeclaration(Type_Token type_, Name name) {
        this.type_ = type_.GetValue();
        this.name = name;
    }
    
    public Name GetName() {
        return name;
    }

    public Type_ GetType_() {
        return type_;
    }
}
