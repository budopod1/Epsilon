using System;
using System.Collections.Generic;

public class VarDeclaration : TreeToken {
    public VarDeclaration(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new VarDeclaration(tokens);
    }
}
