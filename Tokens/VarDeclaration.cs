using System;
using System.Collections.Generic;

public class VarDeclaration : TreeToken {
    public VarDeclaration(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
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
