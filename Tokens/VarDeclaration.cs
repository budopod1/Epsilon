using System;
using System.Collections.Generic;

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

/*
using System;
using System.Collections.Generic;

public class VarDeclaration : TreeToken {
    public VarDeclaration(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new VarDeclaration(tokens);
    }

    public Type_ GetType_() {
        if (this.Count < 2) return null;
        IToken type_token = this[0];
        if (!(type_token is Type_Token)) return null;
        return ((Type_Token)type_token).GetValue();
    }

    public Name GetName() {
        if (this.Count < 2) return null;
        IToken name = this[1];
        if (!(name is Name)) return null;
        return (Name)name;
    }
}
*/
